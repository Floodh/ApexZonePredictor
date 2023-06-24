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

        // Bitmap basemap = DataSource.FormBase();
        // basemap.Save("basemap.png", ImageFormat.Png);

        for (int sample = 0; sample < sampleSize; sample++)
        {

            List<VecPoint> ringCenters = DataSource.GetRingCenters(sample);
            int i = 0;
            foreach (VecPoint ringCenter in ringCenters)
            {
                //Console.WriteLine($"i = {i}");
                Bitmap canvas = new Bitmap($"testmap{i}.png");
                DataSource.DrawCross(canvas, ringCenter.value);
                canvas.Save($"crossmap_{i}.png", ImageFormat.Png);
                i++;
            }


            Console.WriteLine("----- Prediction -----");
            {

                Bitmap canvas = new Bitmap($"basemap.png");
                Console.WriteLine("----- Vector chain -----");

                VectorData vecData = new VectorData(ringCenters.ToArray());
                vecData.Draw(canvas);
                canvas.Save($"Vectors{sample}.png", ImageFormat.Png);

            }

            Console.WriteLine("----- Heatmap -----");
            {
                Bitmap heatmap =  Predictor.GetHeatmap(ringCenters[0], ringCenters[1]);
                heatmap.Save($"heatmap_{sample}.png", ImageFormat.Png);

            }


        }


        Console.WriteLine("end");
        return 0;
    }
}