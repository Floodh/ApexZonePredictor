#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;



//  i hate this class and i should remove it
static class Activity
{

    public const string folder_Output = "Output/";

    public static List<Result> ProcessTestData(string map, string setName)
    {

        int sampleSize;
        if (map == "WE" || map == "SP")
            sampleSize = DataSource.GetSampleSize_ZoneData(map);
        else
            throw new Exception($"Invalid map to process test data: {map}, needs to be SP or WE");


    
        Console.WriteLine("----- Forming base -----");
        Bitmap basemap = DataSource.FormBase(map, setName);


        Console.WriteLine("----- Determining space ----");
        InvalidSpace space;
        {
            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            space = new InvalidSpace(map);
            space.DrawCombined(canvas);
            canvas.Save($"{DataSource.folder_Cache}{map}_Space.png", ImageFormat.Png);
        }

        Console.WriteLine("----- Processing Data -----");

        List<Result> results = new List<Result>(sampleSize);

        for (int sample = 0; sample < sampleSize; sample++)
            if (File.Exists($"{DataSource.folder_ZoneData}{map}/ZoneData_{sample}_{0}.{setName}.png"))  //  will only process of the right set
        {

            Console.WriteLine($"      {sample}");

            List<VecPoint> ringCenters;
            Console.WriteLine("      Searching for ring centers...");
            {
                ringCenters = DataSource.GetRingCenters(map, setName, sample, basemap);
            }

            VectorData vecData;
            Console.WriteLine("      Calculating Vectors...");
            {

                Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                vecData = new VectorData(ringCenters.ToArray());
                vecData.Draw(canvas);
                canvas.Save($"{DataSource.folder_Fragments}Vectors_{map}_{sample}.png", ImageFormat.Png);

            }

            Bitmap heatmap;
            Method m;
            Console.WriteLine("      Creating Heatmap...");
            {
                // heatmap = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                // Heatmap.DrawHeatmap(heatmap, ringCenters[0], ringCenters[1], space);
                m = new Method(vecData);
                heatmap = m.Apply(basemap, vecData, space, map);                
                heatmap.Save($"{DataSource.folder_Fragments}Heatmap_{sample}.png", ImageFormat.Png);

                if (sample == 4)
                {
                    if (m.ringPattern != Method.Pattern.delayedHardPull)
                        Console.WriteLine("            Warning: Incorrect zone pattern for sample: WE 4");
                    if (Math.Abs(vecData.pullAngle_low - Math.PI) > 0.1)
                        Console.WriteLine("            Warning: Incorrect angle for sample: WE 4");
                }
            }

            Console.WriteLine("      Determening result...");
            {
                Result result = new Result(basemap, heatmap, vecData, m, map, setName, sample);
                results.Add(result);

            }


            Console.WriteLine("");


        }

        return results;

    }


    public static void CaptureAllData(string set)
    {
        Console.Write("Drop data start from id (Enter a number): ");
        int dropData_startFrom = int.Parse(Console.ReadLine());
        int dropData_gameCount = dropData_startFrom;

        Console.Write("Zone data start from id (Enter a number): ");
        int zoneData_startFrom = int.Parse(Console.ReadLine());
        int zoneData_gameCount = zoneData_startFrom;

        while (UserDecision($"Gather data? dropData id = {dropData_gameCount}, zone dara id = {zoneData_gameCount} (y/n): ", "y", "n"))
        {


            if (UserDecision("Capture drop data? (y/n): ", "y", "n"))
            {
                Bitmap map = DataSource.CaptureMap();
                map.Save($"{DataSource.folder_DropData}DropData_{dropData_gameCount}.{set}.png", ImageFormat.Png);
                dropData_gameCount++;
            }

            int ring = 0;
            while (UserDecision($"Capture ring {ring} data? (y/n): ", "y", "n"))
            {
                Bitmap map = DataSource.CaptureMap();
                map.Save($"{DataSource.folder_ZoneData}ZoneData_{zoneData_gameCount}_{ring}.{set}.png", ImageFormat.Png);
                ring++;
            }
            if (ring > 0)
            {
                zoneData_gameCount++;
            }

        }

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
            map.Save($"{DataSource.folder_DropData}DropData_{gameCount}.png", ImageFormat.Png);
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
                map.Save($"{DataSource.folder_ZoneData}ZoneData_{gameCount}_{zoneCount}.png", ImageFormat.Png);

                zoneCount++;

            }

            gameCount++;

        }    
    }

    //  same as CaptureZoneData except that the result needs to be saved with a different directory and filename.
    public static void CaptureRingConsoleData()
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
            map.Save($"{DataSource.folder_RingConsoleData}RingConsoleData_{gameCount}.png", ImageFormat.Png);
            gameCount++;

        }    
    }


    private static bool UserDecision(string message, string t, string f)
    {
        while (true)
        {

            Console.Write(message);
            string line = Console.ReadLine();
            if (line == t)
                return true;
            if (line == f)
                return false;
            
        }
    }


    // private static void ValidatePattern()
    // {
        
    // }


}
