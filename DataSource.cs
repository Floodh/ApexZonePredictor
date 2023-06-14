using System.Drawing;
using System.Drawing.Imaging;

static class DataSource
{

    private static readonly Size resolution = new Size(2560, 1440);
    private static readonly Size mapResolution = new Size(1912 - 560, 1352 - 0);
    private static readonly Rectangle mapBounds = new Rectangle(0, 0, mapResolution.Width, mapResolution.Height);
    //  560     0               1912    0

    //  560     1352            1912    1352

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

    private const int edgeMargin = 7;
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

    
    private const int iterations = 10;
    public static Point GetRingCenter(Bitmap edgemap, Point start)
    {
        //  the idea is that we place a cross in the middle of the map
        //  then if parts of the cross overlap with the detected ring
        //  then the cross will be pulled towards those pixels
        //  the closer the distance from the middle of the cross to the
        //  overlapped pixel is, the harder it will pull
        //  after a few iterations of this the cross will be align 
        //  with the ring circle
        Point center = start;
        Point[] moveArray = new Point[4]{new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1)};




        for (int i = 0; i < iterations; i++)
        {

            double pullX = 0.0;
            double pullY = 0.0;

            foreach (Point movePoint in moveArray)
            {


                Point walkPoint = center;
                while (true)
                {
                    walkPoint.Offset(movePoint.X, movePoint.Y);
                    if (!mapBounds.Contains(walkPoint))
                        break;

                    //Console.WriteLine($"Color : {edgemap.GetPixel(walkPoint.X, walkPoint.Y).IsNamedColor}");
                    Color pixel = edgemap.GetPixel(walkPoint.X, walkPoint.Y);
                    if (pixel.R == Color.Purple.R & pixel.G == Color.Purple.G & pixel.B == pixel.B)
                    {
                        int distance = walkPoint.X - center.X + walkPoint.Y - center.Y;
                        distance = Math.Abs(distance);
                        pullX += (double)movePoint.X * (double)distance * 0.005;
                        pullY += (double)movePoint.Y * (double)distance * 0.005;
                        //Console.WriteLine($"    Pulls : {pullX}, {pullY}");
                    }
                }

            }

            // Console.WriteLine($"Pulls : {pullX}, {pullY}");
            // Console.WriteLine($"Old point = {center}");
            center = new Point(center.X + ((int)pullX), center.Y + ((int)pullY));
            // Console.WriteLine($"new point = {center}\n");

        }


        return center;



    }

    public static List<Point> GetRingCenters(Bitmap edgemap)
    {
        List<Point> result = new List<Point>();
        Point center = new Point(edgemap.Width / 2, edgemap.Height / 2);
        for (int i = 0; i < 5; i++)
        {
            center = GetRingCenter(edgemap, center);
            result.Add(center);
        }

        return result;

    }



    public static void DrawCross(Bitmap canvas, Point crossLocation)
    {

        Point[] moveArray = new Point[4]{new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1)};

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

}