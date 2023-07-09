using System.Drawing;
using System.Drawing.Imaging;



//  i hate this class and i should remove it
class Activity
{

    public const string folder_Output = "Output/";

    public static void ProcessRingConsoleData()
    {
        Console.WriteLine("----- Forming base -----");
        Bitmap basemap = DataSource.FormBase();


        Console.WriteLine("----- Determining space ----");
        InvalidSpace space;
        {
            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            space = new InvalidSpace();
            space.DrawCombined(canvas);
            canvas.Save($"{DataSource.folder_Cache}Space.png", ImageFormat.Png);
        }

        Console.WriteLine("----- Processing Data -----");

        VecPoint center = new VecPoint(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);
        Bitmap source = new Bitmap("DataSource/RingConsoleData/RingConsoleData_0.png");

        Bitmap ringConsoleEdgemap = DataSource.FormEdgemap_RingConsole(source, basemap);
        ringConsoleEdgemap.Save($"{DataSource.folder_Fragments}/EdgemapConsole.png", ImageFormat.Png);
        Bitmap normalEdgemap = DataSource.FormEdgemap(source, basemap);
        normalEdgemap.Save($"{DataSource.folder_Fragments}/EdgemapNormal.png", ImageFormat.Png);


        Console.WriteLine("      Searching for ring centers...");

        VecPoint ring1 = DataSource.GetRingCenter(ringConsoleEdgemap, center, 1);
        VecPoint ring2 = DataSource.GetRingCenter(normalEdgemap, ring1 , 2);

        Console.WriteLine("      Vector chain...");
        {

            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            VectorData vecData = new VectorData(ring1, ring2);
            vecData.Draw(canvas);
            canvas.Save($"{folder_Output}C_Vectors.png", ImageFormat.Png);

        }

        Console.WriteLine("      Heatmap...");
        {
            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            Predictor.DrawHeatmap(canvas, ring1, ring2, space);
            canvas.Save($"{folder_Output}C_Heatmap_.png", ImageFormat.Png);
        }

        Console.WriteLine("");
    }


    const int sampleSize = 7;

    public static void ProcessTestData()
    {

    
        Console.WriteLine("----- Forming base -----");
        Bitmap basemap = DataSource.FormBase();


        Console.WriteLine("----- Determining space ----");
        InvalidSpace space;
        {
            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            space = new InvalidSpace();
            space.DrawCombined(canvas);
            canvas.Save($"{DataSource.folder_Cache}Space.png", ImageFormat.Png);
        }

        Console.WriteLine("----- Processing Data -----");

        for (int sample = 0; sample < sampleSize; sample++)
        {

            Console.WriteLine($"      {sample}");

            Console.WriteLine("      Searching for ring centers...");

            List<VecPoint> ringCenters = DataSource.GetRingCenters(sample, basemap);

            Console.WriteLine("      Vector chain...");
            {

                Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                VectorData vecData = new VectorData(ringCenters.ToArray());
                vecData.Draw(canvas);
                canvas.Save($"{DataSource.folder_Fragments}Vectors{sample}.png", ImageFormat.Png);

            }

            Console.WriteLine("      Heatmap...");
            {
                Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                Predictor.DrawHeatmap(canvas, ringCenters[0], ringCenters[1], space);
                canvas.Save($"{DataSource.folder_Fragments}Heatmap_{sample}.png", ImageFormat.Png);
            }

            Console.WriteLine("");


        }

    }


}
