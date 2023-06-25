#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

static class MainClass
{

    const int sampleSize = 7;


    static int Main(string[] args)
    {


        Console.WriteLine("start");

        // DataSource.CaptureDropData();
        // DataSource.CaptureZoneData();

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








        Console.WriteLine("end");
        return 0;
    }
}