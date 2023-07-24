#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;




static class Heatmap
{

    public static readonly Color heatColor = Color.FromArgb(255, 122, 50);

    private const float heatRadius = 28f;

    public static void HeatLine(Bitmap canvas, VecPoint start, VecPoint end)
    {
        //Pen pen = new Pen(Color.Red);
        Brush brush = new SolidBrush(heatColor);
        Graphics g = Graphics.FromImage(canvas);


        Vector2 v2_start = new Vector2(start.X, start.Y);
        Vector2 v2_end = new Vector2(end.X, end.Y);

        for (int i = 0; i < 16; i++)
        {
            float progress = (float)(i) / 15.0f;
            Vector2 drawPoint = Vector2.Lerp(v2_start, v2_end, progress);
            Rectangle drawArea = new Rectangle((int)(drawPoint.X - heatRadius), (int)(drawPoint.Y - heatRadius), (int)(heatRadius * 2), (int)(heatRadius * 2));
            g.FillEllipse(brush, drawArea);
            //g.DrawEllipse(pen, drawArea);

        }

        g.Dispose();
        brush.Dispose();
        //pen.Dispose();

    }

    public static Bitmap Apply(Bitmap basemap, Bitmap heatmap, Rectangle area, InvalidSpace space)
    {
        Bitmap result = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);

        for (int y = area.Y; y < area.Bottom; y++)
        for (int x = area.X; x < area.Right; x++)
        {
            ApplyPixel(basemap, heatmap, space, result, x, y);
        }
        return result;

    }

    public static Bitmap Apply(Bitmap basemap, Bitmap heatmap, Circle circleArea, InvalidSpace space)
    {
        Bitmap result = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
        Rectangle squareArea = new Rectangle((int)(circleArea.x - circleArea.radius), (int)(circleArea.y - circleArea.radius), (int)(circleArea.radius * 2), (int)(circleArea.radius * 2));

        for (int y = squareArea.Y; y < squareArea.Bottom; y++)
        for (int x = squareArea.X; x < squareArea.Right; x++)
        {
            int dx = x - circleArea.X;
            int dy = y - circleArea.Y;
            if (dx * dx + dy * dy < circleArea.radius * circleArea.radius)
            {
                ApplyPixel(basemap, heatmap, space, result, x, y);
            }
                
            
        }
        return result;

    }

    private static void ApplyPixel(Bitmap basemap, Bitmap heatmap, InvalidSpace space, Bitmap result, int x, int y)
    {
        Color pixel = heatmap.GetPixel(x, y);
        result.SetPixel(x, y, basemap.GetPixel(x, y));

        if (pixel == heatColor)
            if (space.IsValidCircle(new Circle(x, y, DataSource.we_ringRadius5)))
                result.SetPixel(x, y, pixel);        
    }



}