using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;


static class Predictor
{
    public static Point PredictFinalZone(Point firstCircle, Point secondCircle)
    {
        //List<Point> result = new List<Point>();
        VecPoint center = new VecPoint(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);


        return Point.Empty;


    }

    public static void DisplayCircleData(VecPoint[] source)
    {

        Bitmap canvas = new Bitmap("basemap.png");

        VectorData data = new VectorData(source);
        data.Draw(canvas);
        canvas.Save("Vectors.png", ImageFormat.Png);

    }

}