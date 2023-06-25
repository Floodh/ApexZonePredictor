#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;


static class Predictor
{

    private const float heatRadius = 22f;



    public static void DrawHeatmap(Bitmap canvas, VecPoint firstCircle, VecPoint secondCircle, InvalidSpace space)
    {
        DrawHeatmap(canvas, new VectorData(firstCircle, secondCircle), space);
    }

    public static void DrawHeatmap(Bitmap canvas, VectorData souce, InvalidSpace space)
    {

        
        Bitmap drawmap = new Bitmap(DataSource.mapResolution.Width, DataSource.mapResolution.Height);
        Bitmap transparent = new Bitmap(DataSource.mapResolution.Width, DataSource.mapResolution.Height);

        if (souce.isSourced)
        {
            Console.WriteLine("Overideing souce!!!!");
        }

        HeatLine(drawmap, souce.G, souce.F);
        HeatLine(drawmap, souce.J, souce.I);

        HeatLine(drawmap, souce.F, souce.J); //  if angle >= 90

        CleanHeat(drawmap, transparent, DataSource.mapBounds, space);

        drawmap.Save($"{DataSource.folder_Fragments}HeatmapTransparent.png", ImageFormat.Png);

        for (int y = 0; y < DataSource.mapResolution.Height; y++)
        for (int x = 0; x < DataSource.mapResolution.Width; x++)     
        {
            Color pixel = drawmap.GetPixel(x, y);
            if (pixel != transparent.GetPixel(x, y))
                canvas.SetPixel(x, y, pixel);
        }   

    }

    private static void HeatLine(Bitmap canvas, VecPoint start, VecPoint end)
    {
        //Pen pen = new Pen(Color.Red);
        Brush brush = new SolidBrush(Color.Firebrick);
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

    public static void CleanHeat(Bitmap canvas, Bitmap basemap, Rectangle area, InvalidSpace space)
    {

        for (int y = area.Y; y < area.Bottom; y++)
        for (int x = area.X; x < area.Right; x++)
        {
            if (canvas.GetPixel(x, y) != basemap.GetPixel(x, y))
            {
                if (!space.IsValidCircle(new Circle(x, y, DataSource.we_ringRadius5)))
                {
                    canvas.SetPixel(x, y, basemap.GetPixel(x, y));
                }
            }
        }

    }




}