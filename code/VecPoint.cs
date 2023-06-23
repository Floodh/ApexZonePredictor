using System.Drawing;

readonly struct VecPoint
{

    public readonly Point value;
    public readonly int X{get { return value.X; }}
    public readonly int Y{get { return value.Y; }}

    public readonly double Length{get{return Math.Sqrt((double)(X * X + Y * Y));}}
    public readonly double LengthSquared{get{return (double)(X * X + Y * Y);}}

    //  is this wrong?
    public readonly double Direction{get{return Math.Atan2(Y, X);}}


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


    //  why did i add this???????
    //  makes no sense since int values are stored rather than floats
    public VecPoint Normalize()
    {
        float ls = value.X * value.X + value.Y * value.Y;
        float invNorm = 1.0f / (float)Math.Sqrt((double)ls);

        return new VecPoint(
            value.X * invNorm,
            value.Y * invNorm);
    }


    public VecPoint Offset(double x, double y)
    {
        return this + new VecPoint(X, y);
    }
    public VecPoint Offset(int x, int y)
    {
        return this + new VecPoint(X, y);
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
    
}