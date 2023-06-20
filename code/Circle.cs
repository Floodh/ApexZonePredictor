using System.Drawing;

//  this will be needed later for the ban location data
readonly struct Circle
{
    readonly Point position = Point.Empty;
    readonly double radius = 0.0;

    public Circle(int x, int y, double radius)
        : this(new Point(x, y), radius)
    {}
    
    public Circle(Point position, double radius)
    {
        this.position = position;
        this.radius = radius;
    }
    public bool Contains(Point position)
    {
        int dx = this.position.X - position.X;
        int dy = this.position.Y - position.Y;
        return ((double)(dx * dx + dy * dy) < this.radius * this.radius);
    }
    public Circle AdjustToElement(string variableName, string valueStr)
    {

        int value = int.Parse(valueStr);

        if (variableName == "radius")
        {
            return new Circle(this.position, value);
        }
        else if (variableName == "x")
        {
            return new Circle(value, this.position.Y, radius);

        }
        else if (variableName == "y")
        {
            return new Circle(this.position.X, value, radius);
        }
        else
        {
            Console.WriteLine($"Warning, adjusting to invalid variable name: {variableName}");
            return this;
        }

    }

    public override string ToString()
    {
        return $"x : {this.position.X}, y : {this.position.Y}, r : {this.radius}";
    }
}