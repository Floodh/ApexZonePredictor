#pragma warning disable CA1416

using System.Drawing;

//  this will be needed later for the ban location data
readonly struct Circle
{
    public readonly double x = 0;
    public readonly double y = 0;
    public readonly double radius {get; init;} = 0.0;
    public readonly int X {get {return (int)x;}}
    public readonly int Y {get {return (int)y;}}
    public readonly Point Location { get {return new Point(X, Y);}}

    public readonly Rectangle DrawRect {get {return new Rectangle(this.X - (int)(this.radius), this.Y - (int)(this.radius), (int)(this.radius * 2), (int)(this.radius * 2));}}

    public Circle(Point position, double radius)
        : this(position.X, position.Y, radius) 
    {}

    public Circle(int x, int y, double radius)
        : this((double)x, (double)y, radius)
    {}
    
    public Circle(double x, double y, double radius)
    {
        this.x = x;
        this.y = y;
        this.radius = radius;
    }


    public bool Contains(Point position)
    {
        int dx = this.X - position.X;
        int dy = this.Y - position.Y;
        return ((double)(dx * dx + dy * dy) < this.radius * this.radius);
    }
    public Circle AdjustToElement(string variableName, string valueStr)
    {

        int value = int.Parse(valueStr);

        if (variableName == "radius")
        {
            return new Circle(this.Location, value);
        }
        else if (variableName == "x")
        {
            return new Circle(value, this.Y, radius);

        }
        else if (variableName == "y")
        {
            return new Circle(this.X, value, double.Parse(valueStr));
        }
        else
        {
            Console.WriteLine($"Warning, adjusting to invalid variable name: {variableName}");
            return this;
        }

    }

    public Circle Offset(double x, double y)
    {
        return new Circle(this.x + x, this.y + y, this.radius);
    }
    public Circle Offset(int x, int y)
    {
        return new Circle(this.x + x, this.y + y, this.radius);
    }
    public Circle Offset(Point position)
    {
        return Offset(position.X, position.Y);
    }

    public override string ToString()
    {
        return $"x : {this.X}, y : {this.Y}, r : {this.radius}";
    }

    //  is this really correct?
    public static Circle operator/(Circle c, int divider)
    {
        return new Circle(c.X / divider, c.Y / divider, c.radius / divider);
    }
    public static Circle operator*(Circle c, double multipler)
    {
        return new Circle(c.x * multipler, c.y * multipler, c.radius * multipler);
    }

}