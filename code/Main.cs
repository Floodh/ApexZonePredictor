using System.Drawing;
using System.Drawing.Imaging;

static class MainClass
{
    static int Main(string[] args)
    {


        Console.WriteLine("start");

        //DataSource.CaptureDropData();

        // Bitmap basemap = DataSource.FormBase();
        // basemap.Save("basemap.png", ImageFormat.Png);

        // Bitmap edgemap = DataSource.FormEdgemap(new Bitmap("ZoneData_1_0.png"), new Bitmap("basemap.png"));
        // Point crossCenter = DataSource.GetRingCenter(edgemap, new Point(edgemap.Width / 2, edgemap.Height / 2));
        // DataSource.DrawCross(edgemap, crossCenter);
        // edgemap.Save("crossmap.png", ImageFormat.Png);


        Bitmap edgemap = DataSource.FormEdgemap(new Bitmap("ZoneData_1_0.png"), new Bitmap("basemap.png"));
        List<Point> ringCenters = DataSource.GetRingCenters(4);
        int i = 0;
        foreach (Point ringCenter in ringCenters)
        {
            //Console.WriteLine($"i = {i}");
            Bitmap canvas = new Bitmap("basemap.png");
            DataSource.DrawCross(canvas, ringCenter);
            canvas.Save($"crossmap_{i}.png", ImageFormat.Png);
            i++;
        }

        Console.WriteLine("end");
        return 0;
    }
}