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
        // Point center = new Point(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);


        // Point vec_centerToFirst = new Point(source[0].X - center.X, source[0].Y - center.Y);
        // Point vec_firstToSecond = new Point(source[1].X - source[0].X, source[1].Y - source[0].Y);
        // Point vec_secondToThird = new Point(source[2].X - source[1].X, source[2].Y - source[1].Y);
        // Point vec_thirdToFourth = new Point(source[3].X - source[2].X, source[3].Y - source[2].Y);
        // Point vec_fourthToFinal = new Point(source[4].X - source[3].X, source[4].Y - source[3].Y);
        
        // Vector2 vec = new Vector2(vec_centerToFirst.X, vec_centerToFirst.Y);
        // float circleRadius = (float)DataSource.we_ringRadius[1];
        // vec = new Vector2(vec.X * ((circleRadius - vec.Length() ) / circleRadius), vec.Y * ((circleRadius - vec.Length())  / circleRadius));
        // Point vec_centerToEdge = new Point(-(int)vec.X, -(int)vec.Y);  

        // Point vec_firstMidSecond = new Point(vec_firstToSecond.X / 2, vec_firstToSecond.Y / 2);
        
        // Console.WriteLine($"vec_centerToFirst : {vec_centerToFirst}");
        // Console.WriteLine($"vec_firstToSecond : {vec_firstToSecond}");
        // Console.WriteLine($"vec_secondToThird : {vec_secondToThird}");
        // Console.WriteLine($"vec_thirdToFourth : {vec_thirdToFourth}");
        // Console.WriteLine($"vec_fourthToFinal : {vec_fourthToFinal}");
        // Console.WriteLine($"vec_centerToEdge  : {vec_centerToEdge}");


        Bitmap canvas = new Bitmap("basemap.png");

        // DrawVector(canvas, center, vec_centerToFirst, Color.Red);
        // DrawVector(canvas, center, vec_centerToEdge, Color.Purple);


        // Point[] vectors = new Point[5]{vec_firstToSecond, vec_secondToThird, vec_thirdToFourth, vec_fourthToFinal, Point.Empty};
        // DrawVectors(canvas, source.ToArray(), vectors);
        VecPoint[] souce = new VecPoint[5];
        for (int i = 0; i < 5; i++)
        {
            souce[i] = new VecPoint(source[i]);
        }

        VectorData data = new VectorData(souce);
        data.Draw(canvas);
        canvas.Save("Vectors.png", ImageFormat.Png);


        //Point vec_firstToFinal = new Point(vec_centerToFirst.X + vec_firstToSecond.X, vec_centerToFirst.Y + vec_firstToSecond.Y);
        //Point finalCircle = new Point(firstCircle.X + vec_firstToFinal.X, firstCircle.Y + vec_firstToFinal.Y);

    }

    // public static void DrawVectors(Bitmap canvas, Point[] points, Point[] vectors)
    // {

    //     if (points.Length != vectors.Length)
    //     {
    //         Console.WriteLine($"WARNING, length not equal, point : {points.Length}, vectors : {vectors.Length}");
    //         return;
    //     }

    //     for (int i = 0; i < points.Length; i++)
    //     {
    //         DrawVector(canvas, points[i], vectors[i], Color.Blue);
    //         Console.WriteLine($"from {points[i]}, with {vectors[i]} should lead to {new Point(points[i].X + vectors[i].X, points[i].Y + vectors[i].Y)}");
    //     }
        
    // }


    private const double stepSize = 0.32;

    public static void DrawVector(Bitmap canvas, VecPoint start, VecPoint end, Color color)
    {

        // Point end = point;
        // end.Offset(vector);

        VecPoint vector = end - start;
        

        VecPoint vecPoint = new VecPoint(vector.value);
        Vector2 v2Start = new Vector2(start.X, start.Y);
        Vector2 v2End = new Vector2(end.X, end.Y);
        Vector2.Lerp(v2Start, v2End, 0.0f);

        VecPoint walkPoint = start;

        double traveledDistance = 0.0;

        while (traveledDistance * traveledDistance < vecPoint.LengthSquared)
        {
            //Console.WriteLine($"    {walkPoint}");

            if (DataSource.mapBounds.Contains(walkPoint.value))
            {
                canvas.SetPixel(walkPoint.X, walkPoint.Y, color);
            }

            traveledDistance += stepSize;
            double progress = traveledDistance / vecPoint.Length;
            Vector2 drawPoint = Vector2.Lerp(v2Start, v2End, (float)progress);
            walkPoint = new VecPoint((int)drawPoint.X, (int)drawPoint.Y);

        }


        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
        {

            VecPoint drawPoint = start;
            drawPoint.Offset(x - 2, y - 2);
            if (DataSource.mapBounds.Contains(drawPoint.value))
                canvas.SetPixel(drawPoint.X, drawPoint.Y, color);
            else
                Console.WriteLine("WARNING, drawPoint is outside of the map");
    
        } 
      
    }


}