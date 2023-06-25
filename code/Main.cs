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

        Bitmap basemap = DataSource.FormBase();

        for (int sample = 0; sample < sampleSize; sample++)
        {

            Console.WriteLine("----- Processing Data -----");

            List<VecPoint> ringCenters = DataSource.GetRingCenters(sample, basemap);

            Console.WriteLine("----- Vector chain -----");
            {

                Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                VectorData vecData = new VectorData(ringCenters.ToArray());
                vecData.Draw(canvas);
                canvas.Save($"{DataSource.folder_Fragments}Vectors{sample}.png", ImageFormat.Png);

            }

            Console.WriteLine("----- Heatmap -----");
            {
                Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
                Predictor.DrawHeatmap(canvas, ringCenters[0], ringCenters[1]);
                canvas.Save($"{DataSource.folder_Fragments}Heatmap_{sample}.png", ImageFormat.Png);
            }


        }

        Console.WriteLine("----- Space ----");
        {
            Bitmap canvas = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
            InvalidSpace space = new InvalidSpace();
            space.Combine(canvas);
            canvas.Save($"{DataSource.folder_Cache}Space.png", ImageFormat.Png);
        }






        Console.WriteLine("end");
        return 0;
    }
}