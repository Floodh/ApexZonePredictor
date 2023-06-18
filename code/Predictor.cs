using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;


static class Predictor
{
    public static Point PredictFinalZone(Point firstCircle, Point secondCircle)
    {
        //List<Point> result = new List<Point>();
        Point center = new Point(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);






        // Console.WriteLine($"vec_centerToFirst : {vec_centerToFirst}");
        // Console.WriteLine($"vec_firstToSecond : {vec_firstToSecond}");
        // Console.WriteLine($"vec_firstToFinal  : {vec_firstToFinal}");
        // Console.WriteLine($"vec_edgeToCenter  : {vec_edgeToCenter}");
        // Console.WriteLine($"finalCircle       : {finalCircle}");

        return Point.Empty;


    }


    // public static double ValidatePrediction(List<Point> sourceZones, List<Point> predictedZones)
    // {


    // }


    public static void DisplayCircleData(List<Point> source)
    {
        Point center = new Point(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);


        Point vec_centerToFirst = new Point(source[0].X - center.X, source[0].Y - center.Y);
        Point vec_firstToSecond = new Point(source[1].X - source[0].X, source[1].Y - source[0].Y);
        Point vec_secondToThird = new Point(source[2].X - source[1].X, source[2].Y - source[1].Y);
        Point vec_thirdToFourth = new Point(source[3].X - source[2].X, source[3].Y - source[2].Y);
        Point vec_fourthToFinal = new Point(source[4].X - source[3].X, source[4].Y - source[3].Y);
        Console.WriteLine($"vec_centerToFirst : {vec_centerToFirst}");
        Console.WriteLine($"vec_firstToSecond : {vec_firstToSecond}");
        Console.WriteLine($"vec_secondToThird : {vec_secondToThird}");
        Console.WriteLine($"vec_thirdToFourth : {vec_thirdToFourth}");
        Console.WriteLine($"vec_fourthToFinal : {vec_fourthToFinal}");



        //Point vec_firstToFinal = new Point(vec_centerToFirst.X + vec_firstToSecond.X, vec_centerToFirst.Y + vec_firstToSecond.Y);
        //Point finalCircle = new Point(firstCircle.X + vec_firstToFinal.X, firstCircle.Y + vec_firstToFinal.Y);

    }

    public static void DrawVectors(Bitmap canvas, Point[] points, Point[] vectors)
    {

    }
}