#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

class Traffic
{

    
    public const string path = DataSource.folder_Cache + "Traffic.png";
    public const int iterations = 15;
    public const int pixelTravelDistance = 150;
    private static Random r = new Random();
    

    private Bitmap image;



    public Traffic(InvalidSpace space)
    {
        // if (File.Exists(path))
        //     this.image = new Bitmap(path);
        // else
            this.image = GenerateTrafficMap(space);
    }

    //  will cache automaticly cache the results
    public static Bitmap GenerateTrafficMap(InvalidSpace space)
    {
        Bitmap image = new Bitmap(DataSource.mapResolution.Width, DataSource.mapResolution.Height);
        image = Iterate(image, space, 0);
        


        image.Save(path, ImageFormat.Png);  //  this will cache the final image, the fragments may be found in the fragment folder
        return image;
    }

    private static Bitmap Iterate(Bitmap image, InvalidSpace space, int depth, int iterations = iterations)
    {

        if (depth >= iterations)
            return image;

        Console.WriteLine($"     Iteration depth : {depth}");


        foreach (int value in Enumerable.Range(0, image.Width * image.Height).OrderBy(x => r.Next()))
        {
            int y = value / image.Width;
            int x = value % image.Width;
            if (space.IsValidCircle(x, y))
            {
                PixelTravel(image, x, y, 0);
            }

        }

        image.Save(DataSource.folder_Fragments + $"Traffic_{depth}.png", ImageFormat.Png);

        return Iterate(image, space, ++depth, iterations);
    }

    static readonly VecPoint center = new VecPoint(DataSource.mapResolution.Width / 2, DataSource.mapResolution.Height / 2);
    private static void PixelTravel(Bitmap trafficMap, int x, int y, int depth, int iterations = pixelTravelDistance)
    {

        if (depth >= iterations)
            return;

        if (x < center.X)
            x += 2;
        else
            x -= 2;
        if (y < center.Y)
            y += 2;
        else
            y -= 2;

        int lowestCongestion = 256;

        int xOffset = 0, yOffset = 0;

        for (int yDiff = -1; yDiff < 2; yDiff++)
        for (int xDiff = -1; xDiff < 2; xDiff++)
        {
            int congestion = trafficMap.GetPixel(x + xDiff, y + yDiff).R;
            if (congestion < lowestCongestion)
            {
                congestion = lowestCongestion;
                xOffset = xDiff;
                yOffset = yDiff;
            }
        }

        //  leave trail
        {
            int congestion = Math.Min(trafficMap.GetPixel(x, y).R + 1, 255);
            int vec_x = Math.Min(Math.Max(trafficMap.GetPixel(x, y).G + xOffset, 0), 255);
            int vec_y = Math.Min(Math.Max(trafficMap.GetPixel(x, y).B + yOffset, 0), 255);

            
            trafficMap.SetPixel(x, y, Color.FromArgb(congestion, vec_x, vec_y));
        }

        PixelTravel(trafficMap, x, y, ++depth);

    }

}