using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

static class DataSource
{

    private static readonly Size resolution = new Size(2560, 1440);
    private static readonly Size mapResolution = new Size(1912 - 560, 1352 - 0);
    private static readonly Rectangle mapBounds = new Rectangle(0, 0, mapResolution.Width, mapResolution.Height);
    //  560     0               1912    0

    //  560     1352            1912    1352
    private const double we_ringRadius0 = 100000;
    private const double we_ringRadius1 = 10 + (1297.0 - 578.0) / 2;
    private const double we_ringRadius2 = 10 + (1264.0 - 877.0) / 2;
    private const double we_ringRadius3 = 10 + (1233.0 - 996.0) / 2;
    private const double we_ringRadius4 = 10 + (1155.0 - 1038.0) / 2;
    private const double we_ringRadius5 = 10 + (1112.0 - 1055.0) / 2;
    private static readonly double[] we_ringRadius = new double[] {we_ringRadius0, we_ringRadius1, we_ringRadius2, we_ringRadius3, we_ringRadius4, we_ringRadius5};

    private static readonly float[] we_ringPullMultipler = new float[6] {0.0025f, 0.0025f, 0.0025f, 0.0025f, 0.0055f, 0.0155f};


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

    
    
    public static Point GetRingCenter(Bitmap edgemap, Point start, Circle bounds, int ring)
    {
        //  the idea is that we place a cross in the middle of the map
        //  then if parts of the cross overlap with the detected ring
        //  then the cross will be pulled towards those pixels
        //  the closer the distance from the middle of the cross to the
        //  overlapped pixel is, the harder it will pull
        //  after a few iterations of this the cross will be align 
        //  with the ring circle
        Point center = start;
        Point[] moveArray = new Point[8]{
            new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1),
            new Point(1, 1), new Point(-1, -1), new Point(-1, 1), new Point(1, -1)
            };
        int radius = (int)we_ringRadius[ring];    



        for (int i = 0; i < iterations; i++)
        {

            Vector2 pull = new Vector2(0.0f, 0.0f);

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
                    if (bounds.Contains(walkPoint))
                    {
                        float dx = (float)(walkPoint.X - center.X);
                        float dy = (float)(walkPoint.Y - center.Y);

                        float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));

                        pull.X += (float)movePoint.X * distance * we_ringPullMultipler[ring];
                        pull.Y += (float)movePoint.Y * distance * we_ringPullMultipler[ring];
                    }
                }

            }
            Console.WriteLine($"Pulls : {pull.X}, {pull.Y}, multiplier = {we_ringPullMultipler[ring]}");
            // Console.WriteLine($"Old point = {center}");
            center = new Point(center.X + ((int)pull.X), center.Y + ((int)pull.Y));
            // Console.WriteLine($"new point = {center}\n");

        }


        return center;



    }

    public static List<Point> GetRingCenters(int gameId)
    {

        List<Point> result = new List<Point>();
        Point center = new Point(mapResolution.Width / 2, mapResolution.Height / 2);
        for (int i = 0; i < 5; i++)
        {
            if (!File.Exists($"ZoneData_{gameId}_{i}.png"))
                break;
            Bitmap edgemap  = DataSource.FormEdgemap(new Bitmap($"ZoneData_{gameId}_{i}.png"), new Bitmap("basemap.png"));
            edgemap.Save($"testmap{i}.png", ImageFormat.Png);
            center = GetRingCenter(edgemap, center, new Circle(center, we_ringRadius[i]), i + 1);
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

}