using System.Drawing;
using System.Drawing.Imaging;

static class DataSource
{

    private static readonly Size resolution = new Size(2560, 1440);
    public static readonly Size mapResolution = new Size(1912 - 560, 1352 - 0);
    public static readonly Rectangle mapBounds = new Rectangle(0, 0, mapResolution.Width, mapResolution.Height);
    //  560     0               1912    0

    //  560     1352            1912    1352
    public const double we_ringRadius0 = 100000;
    public const double we_ringRadius1 = 10 + (1297.0 - 578.0) / 2;
    public const double we_ringRadius2 = 10 + (1264.0 - 877.0) / 2;
    public const double we_ringRadius3 = 10 + (1233.0 - 996.0) / 2;
    public const double we_ringRadius4 = 10 + (1155.0 - 1038.0) / 2;
    public const double we_ringRadius5 = 10 + (1112.0 - 1055.0) / 2;
    public static readonly double[] we_ringRadius = new double[] {we_ringRadius0, we_ringRadius1, we_ringRadius2, we_ringRadius3, we_ringRadius4, we_ringRadius5};

    private static readonly float[] we_ringPullMultipler = new float[6] {0.0025f, 0.0025f, 0.0025f, 0.0025f, 0.0055f, 0.0155f};


    private static readonly Circle we_base_firstInvalidCircle = new Circle(808, 800, 10.77);    //  the radius here is not accurate, but it gives better results for some reason.
    private const int iterations = 25;
    

    private static Bitmap CaptureMap()
    {

        Bitmap bitmap = new Bitmap(mapResolution.Width, mapResolution.Height);
        
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.CopyFromScreen(560,0, 0,0, mapResolution);
        }

        Console.WriteLine("Captured screenshot");
        return bitmap;
    }

    //  this data is easier to use to create the base with
    public static void CaptureDropData()
    {
        Console.Write("Which game id to start from?(Enter a number): ");
        int startFrom = int.Parse(Console.ReadLine());


        Bitmap map = DataSource.CaptureMap();

        string line;
        int gameCount = startFrom;

        while(true)
        {
            
            Console.Write("Capture annother game? (y/n): ");
            line = Console.ReadLine();
            if (line != "y")
                break;

            map = DataSource.CaptureMap();
            map.Save($"DropData_{gameCount}.png", ImageFormat.Png);
            gameCount++;

        }   
    }

    public static void CaptureZoneData()
    {

        Console.Write("Which game id to start from?(Enter a number): ");
        int startFrom = int.Parse(Console.ReadLine());


        Bitmap map = DataSource.CaptureMap();

        string line;
        int gameCount = startFrom;

        while(true)
        {
            
            Console.Write("Capture annother game? (y/n): ");
            line = Console.ReadLine();
            if (line != "y")
                break;

            int zoneCount = 0;
            while (true)
            {

                Console.Write("Capture annother zone? (y/n): ");
                line = Console.ReadLine();
                if (line != "y")
                    break;

                map = DataSource.CaptureMap();
                map.Save($"ZoneData_{gameCount}_{zoneCount}.png", ImageFormat.Png);

                zoneCount++;

            }

            gameCount++;

        }    
    }


    //  based on the sample data
    //  generate a base image which is uneffected by replicators, rings or any other dynamic features
    //  this base image can then be used to simplify other tasks
    public static Bitmap FormBase()
    {

        Bitmap baseMap = new Bitmap(mapResolution.Width, mapResolution.Height);
        Bitmap[] zoneData = new Bitmap[10];
        int sampleSize = 0;

        for (int i = 0; i < 10; i++)
        {

            string path = $"DropData_{i}.png";
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

        edgemap.Save($"EdgeMap.png", ImageFormat.Png);

        return edgemap;
    }

    
    
    public static VecPoint GetRingCenter(Bitmap edgemap, VecPoint start, int ring)
    {


        VecPoint center = start;
        VecPoint[] moveArray = new VecPoint[8]{
            new VecPoint(1, 0), new VecPoint(-1, 0), new VecPoint(0, 1), new VecPoint(0, -1),
            new VecPoint(1, 1), new VecPoint(-1, -1), new VecPoint(-1, 1), new VecPoint(1, -1)
            };
        int radius = (int)we_ringRadius[ring];  


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

    public static List<VecPoint> GetRingCenters(int gameId)
    {

        List<VecPoint> result = new List<VecPoint>();
        VecPoint center = new VecPoint(mapResolution.Width / 2, mapResolution.Height / 2);
        for (int i = 0; i < 5; i++)
        {
            if (!File.Exists($"ZoneData_{gameId}_{i}.png"))
                break;
            Bitmap edgemap  = DataSource.FormEdgemap(new Bitmap($"ZoneData_{gameId}_{i}.png"), new Bitmap("basemap.png"));
            center = GetRingCenter(edgemap, center, i + 1);
            Console.WriteLine(center);
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


    public static Circle[] LoadInvalidZoneData()
    {
        List<Circle> zones = new List<Circle>();

        string htmlText = File.ReadAllText("InvalidZones_html");

        int startIndex = htmlText.IndexOf("[");
        int endIndex = htmlText.IndexOf("]");
        int length = endIndex - startIndex;

        string rawData = htmlText.Substring(startIndex + 1, length - 1);

        string[] elements = rawData.Split(",{");
        foreach (string element in elements)
        {

            Circle circle = new Circle();

            bool buildAlias = false;
            string aliasStr = "";
            bool buildValue = false;
            string valueStr = "";

            foreach (char c in element)
            {
                if (buildAlias)
                {
                    if (c == '"')
                    {
                        buildAlias = false;
                    }
                    else 
                    {
                        aliasStr += c;
                    }

                }
                else if (buildValue)
                {
                    if (c == ',' || c == '}')
                    {
                        circle = circle.AdjustToElement(aliasStr, valueStr);
                        buildValue = false;
                        aliasStr = "";
                        valueStr = "";

                    }
                    else
                    {
                        valueStr += c;
                    }
                    

                }
                else 
                {
                    if (c == '"')
                    {
                        buildAlias = true;
                    }
                    else if (c == ':')
                        buildValue = true;

                }



            }

            zones.Add(circle);

        }

        return zones.ToArray();

    }

    public static Circle[] ConvertInvalidZoneData(Circle[] zones)
    {



        Circle[] result = new Circle[zones.Length];

        double dradius = zones[0].radius - we_base_firstInvalidCircle.radius;

        double radiusMultipler = we_base_firstInvalidCircle.radius / zones[0].radius;

        Circle decreasedCircle = zones[0] * radiusMultipler; 

        double dx = we_base_firstInvalidCircle.X - decreasedCircle.X;
        double dy = we_base_firstInvalidCircle.Y - decreasedCircle.Y;

        Console.WriteLine($"radiusMultipler : {radiusMultipler}, offset after decrement : {dx}, {dy}");

        int count = 0;
        foreach (Circle zone in zones)
        {
            result[count] = zone * radiusMultipler;
            result[count] = result[count].Offset(dx, dy);
            count++;
        }

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