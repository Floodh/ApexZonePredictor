#pragma warning disable CA1416


using System.Drawing;
using System.Drawing.Imaging;


//  prediction method
class Method
{

    public enum Pattern
    {
        none,
        fishhook,
        halfPull_45degress, //  acts like a hlaf moon into a dounghnut
        floatPull_80degress
    }

    public readonly Pattern ringPattern;

    public Method(VectorData vectorData)
    {
        Console.WriteLine("Trying to detect zone pattern...");

        // if (vectorData.pullAngle_low < 47.5 &&
        //     vectorData.pullLength 



        // )
        // else
        // {
        //     this.ringPattern = Pattern.none;
        // }

    }

    public Method(Pattern ringPattern)
    {
        this.ringPattern = ringPattern;
    }

    public Bitmap Apply(Bitmap basemap, VectorData vecData, InvalidSpace space)
    {

        Console.WriteLine($"          diff : {vecData.diffAngle / Math.PI} PI");
        Console.WriteLine($"          c angle : {vecData.pullAngle / Math.PI} PI");
        Console.WriteLine($"          c angle low : {vecData.pullAngle_low / Math.PI} PI");
        Console.WriteLine($"          c angle high : {vecData.pullAngle_high / Math.PI} PI");

        Console.WriteLine($"          Applying method for {this.ringPattern} pattern");

        Bitmap heatmap;

        switch (this.ringPattern)
        {
            // case Pattern.fishhook:
            //     break;
            case Pattern.floatPull_80degress:
                heatmap = Apply_floatPull_80degress(basemap, vecData, space);
                break;
            // case Pattern.halfPull_45degress:
            //     break;
            default:
                Console.WriteLine("          Does not support this pattern, will use defualt method");
                heatmap = Apply_default(basemap, vecData, space);
                break;
        }

        return heatmap;

    }

    private Bitmap Apply_floatPull_80degress(Bitmap basemap, VectorData vecData, InvalidSpace space)
    {
        Bitmap heatmap; // = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
        Bitmap transparentHeatmap = new Bitmap(basemap.Width, basemap.Height);

        VecPoint start = vecData.F;
        VecPoint end = vecData.J;

        Heatmap.HeatLine(transparentHeatmap, start, end);

        transparentHeatmap.Save(DataSource.folder_Fragments + "transparentHeatmap.png", ImageFormat.Png);
  
        heatmap = Heatmap.Apply(basemap, transparentHeatmap, DataSource.mapBounds, space);
        heatmap.Save(DataSource.folder_Fragments + "transparentHeatmap_heatmap.png", ImageFormat.Png);

        return heatmap;
    }

    // private Bitmap Apply_halfPull_45degress(Bitmap heatmap, VectorData vecData)
    // {

    // }

    public Bitmap Apply_default(Bitmap basemap, VectorData vecData, InvalidSpace space)
    {
        Bitmap heatmap = basemap.Clone(new Rectangle(0, 0, basemap.Width, basemap.Height), basemap.PixelFormat);
        Heatmap.DrawHeatmap(heatmap, vecData.firstCircle, vecData.secondCircle, space);
        return heatmap;
    }





}