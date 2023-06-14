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

        Bitmap edgemap = DataSource.FormEdgemap(new Bitmap("ZoneData_1_0.png"), new Bitmap("basemap.png"));
        Point crossCenter = DataSource.GetRingCenter(edgemap, new Point(edgemap.Width / 2, edgemap.Height / 2));
        DataSource.DrawCross(edgemap, crossCenter);
        edgemap.Save("crossmap.png", ImageFormat.Png);

        Console.WriteLine("end");
        return 0;
    }
}