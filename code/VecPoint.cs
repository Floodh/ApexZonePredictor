#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

readonly struct VecPoint
{

    public readonly Point value;
    public readonly int X{get { return value.X; }}
    public readonly int Y{get { return value.Y; }}

    public readonly double Length{get{return Math.Sqrt((double)(X * X + Y * Y));}}
    public readonly double LengthSquared{get{return (double)(X * X + Y * Y);}}

    //  is this wrong?
    public readonly double Direction{get{return Math.Atan2(Y, X);}}

    private const double stepSize = 0.32;

    public VecPoint(double x, double y)
        : this((int)x, (int)y)
    {}

    public VecPoint(int x, int y)
        : this(new Point(x, y))
    {}

    public VecPoint(Point point)        //  primary
    {
        this.value = point;
    }

    public VecPoint(Point from, Point to)
        : this(to.X - from.X, to.Y - from.Y)
    {}

    public VecPoint Offset(double x, double y)
    {
        return this + new VecPoint(x, y);
    }
    public VecPoint Offset(int x, int y)
    {
        return this + new VecPoint(x, y);
    }
    public VecPoint Offset(Point point)
    {
        return this + new VecPoint(point);
    }
    public VecPoint Offset(VecPoint point)
    {
        return this + point;
    }

    public override string ToString()
    {
        return this.value.ToString();
    }


    public static VecPoint operator +(VecPoint p1, VecPoint p2)
    {
        return new VecPoint(p1.X + p2.X, p1.Y + p2.Y);
    }
    public static VecPoint operator -(VecPoint p1, VecPoint p2)
    {
        return new VecPoint(p1.X - p2.X, p1.Y - p2.Y);
    }
    public static VecPoint operator *(VecPoint p, int multipler)
    {
        return new VecPoint(p.X * multipler, p.Y * multipler);
    }
    public static VecPoint operator /(VecPoint p, int divide)
    {
        return new VecPoint(p.X / divide, p.Y / divide);
    }

    public static int Dot(VecPoint value1, VecPoint value2)
    {
        return value1.X * value2.X + value1.Y * value2.Y;
    }

    public static VecPoint FromDirectionAndLength(double length, double direction)
    {
        return new VecPoint(Math.Sin(length) * direction, Math.Cos(length) * direction);

    }

    //  return points on line AB closest to P
    public static VecPoint GetClosestPoint(VecPoint A, VecPoint B, VecPoint P)
    {
        PointF pt = new PointF(P.X, P.Y); 
        PointF p1 = new PointF(A.X, A.Y); 
        PointF p2 = new PointF(B.X, B.Y);
        PointF result;

        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        if ((dx == 0) && (dy == 0))
        {
            throw new Exception("Not a line segment");
        }

        // Calculate the t that minimizes the distance.
        float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) /
            (dx * dx + dy * dy);

        // See if this represents one of the segment's
        // end points or a point in the middle.
        if (t < 0)
        {
            result = new PointF(p1.X, p1.Y);
            dx = pt.X - p1.X;
            dy = pt.Y - p1.Y;
        }
        else if (t > 1)
        {
            result = new PointF(p2.X, p2.Y);
            dx = pt.X - p2.X;
            dy = pt.Y - p2.Y;
        }
        else
        {
            result = new PointF(p1.X + t * dx, p1.Y + t * dy);
            dx = pt.X - result.X;
            dy = pt.Y - result.Y;
        }
        

        return new VecPoint(result.X, result.Y);
    }

    public static void DrawVector(Bitmap canvas, VecPoint start, VecPoint end, Color color)
    {



        VecPoint vector = end - start;
        

        VecPoint vecPoint = new VecPoint(vector.value);
        Vector2 v2Start = new Vector2(start.X, start.Y);
        Vector2 v2End = new Vector2(end.X, end.Y);
        Vector2.Lerp(v2Start, v2End, 0.0f);

        VecPoint walkPoint = start;

        double traveledDistance = 0.0;

        while (traveledDistance * traveledDistance < vecPoint.LengthSquared)
        {

            if (DataSource.mapBounds.Contains(walkPoint.value))
            {
                canvas.SetPixel(walkPoint.X, walkPoint.Y, color);
            }

            traveledDistance += stepSize;
            double progress = traveledDistance / vecPoint.Length;
            Vector2 drawPoint = Vector2.Lerp(v2Start, v2End, (float)progress);
            walkPoint = new VecPoint((int)drawPoint.X, (int)drawPoint.Y);

        }

 

        DrawSquare(canvas, start, color);
        DrawSquare(canvas, end, color);
               
    }

    private static void DrawSquare(Bitmap canvas, VecPoint center, Color color)
    {
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
        {

            VecPoint drawPoint = center;
            drawPoint = drawPoint.Offset(x - 2, y - 2);
            if (DataSource.mapBounds.Contains(drawPoint.value))
                canvas.SetPixel(drawPoint.X, drawPoint.Y, color);
            else
                Console.WriteLine("WARNING, drawPoint is outside of the map");
    
        }            
    }

    public static readonly VecPoint Empty = new VecPoint(0, 0);

}