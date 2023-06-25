using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;


static class Predictor
{

    private const float heatRadius = 22f;



    public static void DrawHeatmap(Bitmap canvas, VecPoint firstCircle, VecPoint secondCircle)
    {
        DrawHeatmap(canvas, new VectorData(firstCircle, secondCircle));
    }

    public static void DrawHeatmap(Bitmap canvas, VectorData souce)
    {
        if (souce.isSourced)
        {
            Console.WriteLine("Overideing souce!!!!");
        }

        HeatLine(canvas, souce.G, souce.F);
        HeatLine(canvas, souce.J, souce.I);

        HeatLine(canvas, souce.F, souce.J); //  if angle >= 90

    }



    private static void HeatLine(Bitmap canvas, VecPoint start, VecPoint end)
    {
        Pen pen = new Pen(Color.Red);
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
            g.DrawEllipse(pen, drawArea);

        }

        g.Dispose();
        brush.Dispose();
        pen.Dispose();

    }




}