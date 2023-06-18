using System.Drawing;

readonly struct VecPoint
{

    public readonly Point value;
    public readonly int X{get { return value.X; }}
    public readonly int Y{get { return value.Y; }}

    public readonly double Length{get{return Math.Sqrt((double)(X * X + Y * Y));}}
    public readonly double Direction{get{return Math.Atan2(Y, X);}}


    VecPoint(double x, double y)
        : this((int)x, (int)y)
    {}

    VecPoint(int x, int y)
        : this(new Point(x, y))
    {}

    VecPoint(Point point)
    {
        this.value = point;
    }

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


    public static VecPoint FromDirectionAndLength(double length, double direction)
    {
        return new VecPoint(Math.Sin(length) * direction, Math.Cos(length) * direction);

    }

    
    


}