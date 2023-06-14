using System.Drawing;

struct Circle
{
    Point position = Point.Empty;
    double radius = 0.0;
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
}