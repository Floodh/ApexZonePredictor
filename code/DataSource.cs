#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

static class DataSource
{

    public const string    folder_Cache        = "Cache/";
    //public const string        file_Basemap        = "Basemap/";

    public const string     folder_DataSource   = "DataSource/";
    public const string         folder_DropData         = folder_DataSource + "DropData/";
    public const string         folder_Space            = folder_DataSource + "Space/";
    public const string         folder_ZoneData         = folder_DataSource + "ZoneData/";
    public const string         folder_RingConsoleData  = folder_DataSource + "RingConsoleData/";

    public const string    folder_Fragments    = "Fragments/";




    private static readonly Size resolution = new Size(2560, 1440);
    public static readonly Size mapResolution = new Size(1912 - 560, 1352 - 0);
    public static readonly Rectangle mapBounds = new Rectangle(0, 0, mapResolution.Width, mapResolution.Height);
    //  560     0               1912    0

    //  560     1352            1912    1352
    public const double we_ringRadius0 = 100000;
    public const double we_ringRadius1 = 2 + (1297.0 - 578.0) / 2;
    public const double we_ringRadius2 = 2 + (1264.0 - 877.0) / 2;
    public const double we_ringRadius3 = 2 + (1233.0 - 996.0) / 2;
    public const double we_ringRadius4 = 2 + (1155.0 - 1038.0) / 2;
    public const double we_ringRadius5 = 2 + (1112.0 - 1055.0) / 2;
    public static readonly double[] we_ringRadius = new double[] {we_ringRadius0, we_ringRadius1, we_ringRadius2, we_ringRadius3, we_ringRadius4, we_ringRadius5};

    private static readonly float[] we_ringPullMultipler = new float[6] {0.0025f, 0.0025f, 0.0025f, 0.0025f, 0.0055f, 0.0155f};


    private static readonly Circle we_base_firstInvalidCircle = new Circle(808, 800, 10.77);    //  the radius here is not accurate, but it gives better results for some reason.
    private const int iterations = 25;
    

    public static Bitmap CaptureMap()
    {

        Bitmap bitmap = new Bitmap(mapResolution.Width, mapResolution.Height);
        
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(560,0, 0,0, mapResolution);
        }

        Console.WriteLine("Captured screenshot");
        return bitmap;
    }

    //  based on the sample data
    //  generate a base image which is uneffected by replicators, rings or any other dynamic features
    //  this base image can then be used to simplify other tasks
    public static Bitmap FormBase(string map, string setName)
    {

        string cachedBasemapPath = $"{DataSource.folder_Cache}{map}_Basemap.{setName}.png";

        if (File.Exists(cachedBasemapPath))
            return new Bitmap(cachedBasemapPath);
        

        Bitmap baseMap = new Bitmap(mapResolution.Width, mapResolution.Height);
        Bitmap[] zoneData = new Bitmap[10];
        int sampleSize = 0;

        for (int i = 0; i < 10; i++)    //  will never need more than 10 bitmaps for a good enough basemap
        {

            string path = $"{folder_DropData}{map}/DropData_{i}.{setName}.png";
            if (File.Exists(path))
            {
                zoneData[i] = new Bitmap(path);
                sampleSize = i + 1;
            }
            else
            {
                break;
            }

        }

        if (sampleSize < 3)
        {
            Console.WriteLine($"WARNING: sample size is very low {sampleSize}");
        }


        for (int y = 0; y < mapResolution.Height; y++)
        {
            for (int x = 0; x < mapResolution.Width; x++)
            {

                Color color = Color.White;
                int frequency = -1;

                for (int i = 0; i < sampleSize; i++)
                {

                    Color pixel = zoneData[i].GetPixel(x, y);
                    int newFreq = 0;


                    for (int j = 0; j < sampleSize; j++)
                        if (j != i)
                    {
                        //  if duplicate pixel exist
                        if (pixel == zoneData[j].GetPixel(x, y))
                            newFreq++;
                    }

                    //  if theres more duplicates
                    if (newFreq > frequency)
                    {
                        frequency = newFreq;
                        color = pixel;
                    }

                }

                //  by this point we have found the most common color
                baseMap.SetPixel(x, y, color);


            }
        }

        //  cache the bitmap
        baseMap.Save(cachedBasemapPath, ImageFormat.Png);
        return baseMap;
    }

    private const int edgeMargin = 2;
    public static Bitmap FormEdgemap(Bitmap source, Bitmap basemap)
    {


        Bitmap edgemap = basemap.Clone(new Rectangle(Point.Empty, mapResolution), basemap.PixelFormat);

        for (int y = 0; y < source.Height; y++)
        for (int x = 0; x < source.Width; x++)
        {
            Color sourceColor = source.GetPixel(x, y);
            Color baseColor = basemap.GetPixel(x, y);
            int diffR = sourceColor.R - baseColor.R;
            int diffG = sourceColor.G - baseColor.G;
            int diffB = sourceColor.B - baseColor.B;
            if (diffR > edgeMargin & diffG > edgeMargin & diffB > edgeMargin)
            {
                edgemap.SetPixel(x, y, Color.Purple);
            }
        }
        return edgemap;
    }

    //  forms an edge map of the green circle
    public static Bitmap FormEdgemap_RingConsole(Bitmap source, Bitmap basemap)
    {
        Bitmap edgemap = basemap.Clone(new Rectangle(Point.Empty, mapResolution), basemap.PixelFormat);
        
        for (int y = 0; y < source.Height; y++)
        for (int x = 0; x < source.Width; x++)
        {
            Color sourceColor = source.GetPixel(x, y);
            Color baseColor = basemap.GetPixel(x, y);
            int diffR = sourceColor.R - baseColor.R;
            int diffG = sourceColor.G - baseColor.G;
            int diffB = sourceColor.B - baseColor.B;
            if (diffR < 30 & diffG > edgeMargin + 12 & diffB < 25)       //  THIS NEEDS TO BE DIFFERENT
            {
                edgemap.SetPixel(x, y, Color.Purple);
            }
        }

        edgemap.Save($"{folder_Fragments}Edgemap.png", ImageFormat.Png);  //  for debug

        return edgemap;        

    }

    

    //  can find the centers of any circle as long as the edgemap is good enough
    //  does not load any images on its own
    public static VecPoint GetRingCenter(Bitmap edgemap, VecPoint start, int ring)
    {


        VecPoint center = start;
        VecPoint[] moveArray = new VecPoint[8]{
            new VecPoint(1, 0), new VecPoint(-1, 0), new VecPoint(0, 1), new VecPoint(0, -1),
            new VecPoint(1, 1), new VecPoint(-1, -1), new VecPoint(-1, 1), new VecPoint(1, -1)
            };
        int radius = (int)we_ringRadius[ring] + 6;  


        for (int i = 0; i < iterations; i++)
        {
            

            VecPoint pullPoint = VecPoint.Empty;
            foreach (VecPoint offsetPoint in moveArray)
            {

                //  point closests to the radius
                VecPoint edge = center;
                double distance = 0.0;

                VecPoint walkPoint = center;

                while (true)
                {
                    walkPoint = walkPoint.Offset(offsetPoint);
                    if (!mapBounds.Contains(walkPoint.value))
                        break;
                    
                    Color pixel = edgemap.GetPixel(walkPoint.X, walkPoint.Y);
                    if (pixel.R == Color.Purple.R & pixel.G == Color.Purple.G & pixel.B == pixel.B)
                    {
                        int dx = walkPoint.X - center.X;
                        int dy = walkPoint.Y - center.Y;
                        double newDistance = Math.Sqrt((double)(dx * dx + dy * dy));

                        if (Math.Abs(radius - newDistance) < Math.Abs(radius - distance))
                        {
                            edge = walkPoint;
                            distance = newDistance;
                        }

                    }
                    
                    
                }

                //  just add the point immediately, we will get the avg later
                pullPoint = new VecPoint(pullPoint.X + edge.X, pullPoint.Y + edge.Y);

            }




            pullPoint = new VecPoint(
                pullPoint.X / moveArray.Length, 
                pullPoint.Y / moveArray.Length);
            center = pullPoint;

        }

        return center;

    }

    public static List<VecPoint> GetRingCenters(string map, string setName, int gameId, Bitmap basemap)
    {

        List<VecPoint> result = new List<VecPoint>();
        VecPoint center = new VecPoint(mapResolution.Width / 2, mapResolution.Height / 2);



        for (int i = 0; i < 5; i++)
        {
            string filePath = $"{folder_ZoneData}{map}/ZoneData_{gameId}_{i}.{setName}.png";
            if (!File.Exists(filePath))
                break;
            Bitmap edgemap  = DataSource.FormEdgemap(new Bitmap(filePath), basemap);
            edgemap.Save($"{DataSource.folder_Fragments}Edgemap_{gameId}_{i}.png", ImageFormat.Png);
            center = GetRingCenter(edgemap, center, i + 1);
            //Console.WriteLine(center);
            result.Add(center);

        }

        return result;

    }



    public static void DrawCross(Bitmap canvas, Point crossLocation)
    {

        Point[] moveArray = new Point[8]{
            new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1),
            new Point(1, 1), new Point(-1, -1), new Point(-1, 1), new Point(1, -1)
            };
        foreach (Point movePoint in moveArray)
        {

            Point walkPoint = crossLocation;
            while (true)
            {
                walkPoint.Offset(movePoint.X, movePoint.Y);
                if (!mapBounds.Contains(walkPoint))
                    break;
                canvas.SetPixel(walkPoint.X, walkPoint.Y, Color.White);

            }

        }        

    }

    public static Bitmap LoadInvalidZones()
    {
        int x = 208, y = 208, width = 1231 - x, height = 1231 - y;

        Bitmap bannedMap = new Bitmap($"{folder_Space}Banned.png");
        Bitmap bannedMap_cropped = bannedMap.Clone(new Rectangle(x, y, width, height), bannedMap.PixelFormat);
        Bitmap bannedMap_resized = new Bitmap(bannedMap_cropped, new Size(1352, 1352));
           
        Bitmap baseMap = new Bitmap($"{folder_Space}Base.png");
        Bitmap baseMap_cropped = baseMap.Clone(new Rectangle(x, y, width, height), baseMap.PixelFormat);
        Bitmap baseMap_resized = new Bitmap(baseMap_cropped, new Size(1352, 1352));

        Bitmap result = new Bitmap(mapResolution.Width, mapResolution.Height);

        for (y = 0; y < mapResolution.Height; y++)
        for (x = 0; x < mapResolution.Width; x++)
        {
            
            Color pixelData = bannedMap_resized.GetPixel(x, y);
            Color pixelBase = baseMap_resized.GetPixel(x, y);

            if (pixelData.R - pixelBase.R > 5)
            {
                result.SetPixel(x, y, Color.Red);
            }
        }

        result.Save($"{DataSource.folder_Fragments}OnlyBanned.png", ImageFormat.Png);     //  debug
        return result;

    }

    public static void DrawCircles(Bitmap canvas, Circle[] circles)
    {
        Pen pen = new Pen(Color.Red);
        Graphics formGraphics = Graphics.FromImage(canvas);
        
       
        foreach (Circle circle in circles)
        {
            int x,y,w,h;
            x = circle.X - (int)(circle.radius);
            y = circle.Y - (int)(circle.radius);
            w = (int)(circle.radius);
            h = w;

            formGraphics.DrawEllipse(pen, new Rectangle(x, y, w, h));
        }

        pen.Dispose();
        formGraphics.Dispose();

    }

}