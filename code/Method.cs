#pragma warning disable CA1416


using System.Drawing;
using System.Drawing.Imaging;


//  prediction method
class Method
{

    public enum Pattern
    {
        none,
        floatPull,          //  45 
        halfMoonDoughnut,   //  acts like a half moon into a dounghnut
        delayedHardPull     //  when all centers align along one line
    }

    public readonly Pattern ringPattern;

    public Method(VectorData vectorData)
        : this(DetectPattern(vectorData))
    {}

    public Method(Pattern ringPattern)
    {
        this.ringPattern = ringPattern;
    }

    public Bitmap Apply(Bitmap basemap, VectorData vecData, InvalidSpace space, string map)
    {

        Console.WriteLine($"          diff : {vecData.diffAngle / Math.PI} PI");
        Console.WriteLine($"          c angle : {vecData.pullAngle / Math.PI} PI");
        Console.WriteLine($"          c angle low : {vecData.pullAngle_low / Math.PI} PI");
        Console.WriteLine($"          c angle high : {vecData.pullAngle_high / Math.PI} PI");

        Console.WriteLine($"          mid to c1   : {vecData.vec_centerToFirst.Length}");
        Console.WriteLine($"          c1 to c2    : {vecData.vec_firstToSecond.Length}");
        Console.WriteLine($"          mid to edge : {vecData.vec_centerToEdge.Length}");

        Console.WriteLine($"          Applying method for {this.ringPattern} pattern");

        Bitmap heatmap;

        switch (this.ringPattern)
        {
            // case Pattern.fishhook:
            //     break;
            case Pattern.floatPull:
                heatmap = Apply_floatPull(basemap, vecData, space, map);
                break;
            case Pattern.halfMoonDoughnut:
                heatmap = Apply_halfMoonDoughnut(basemap, vecData, space, map);
                break;
            case Pattern.delayedHardPull:
                heatmap = Apply_delayedHardPull(basemap, vecData, space, map);
                break;

            default:
                Console.WriteLine("          Does not support this pattern, will use defualt method");
                heatmap = Apply_default(basemap, vecData, space);
                break;
        }

        return heatmap;

    }

    //  does not work for true counter pull, or hard pulls
    private Bitmap Apply_floatPull(Bitmap basemap, VectorData vecData, InvalidSpace space, string map)
    {
        if (vecData.isSourced)
            Console.WriteLine("               Overideing souce!!!!");  

        double radius;
        if (map == "WE")
            radius = DataSource.sp_ringRadius4;
        else if (map == "SP")
            radius = DataSource.sp_ringRadius4;
        else
            throw new ArgumentException($"Invalid map given for method : {map}");

        Bitmap heatmap = Apply_default(basemap, vecData, space);
        heatmap = Heatmap.Apply(basemap, heatmap, new Circle(vecData.M.value, radius), space);


        return heatmap;
    }

    private Bitmap Apply_halfMoonDoughnut(Bitmap basemap, VectorData vecData, InvalidSpace space, string map)
    {
        if (vecData.isSourced)
            Console.WriteLine("               Overideing souce!!!!");  

        double radius;
        if (map == "WE")
            radius = DataSource.sp_ringRadius3;
        else if (map == "SP")
            radius = DataSource.sp_ringRadius3;
        else
            throw new ArgumentException($"Invalid map given for method : {map}");


        Bitmap heatmap = Apply_default(basemap, vecData, space);
        heatmap = Heatmap.Apply(basemap, heatmap, new Circle(vecData.I.value, radius), space);


        return heatmap;
    }

    public Bitmap Apply_delayedHardPull(Bitmap basemap, VectorData vecData, InvalidSpace space, string map)
    {

        if (vecData.isSourced)
            Console.WriteLine("               Overideing souce!!!!");

        Bitmap heatmap = Apply_default(basemap, vecData, space);

        return heatmap;        

    }

    public Bitmap Apply_default(Bitmap basemap, VectorData vecData, InvalidSpace space)
    {
        if (vecData.isSourced)
            Console.WriteLine("               Overideing souce!!!!");

        Bitmap transparentHeatmap = new Bitmap(basemap.Width, basemap.Height);

        Heatmap.HeatLine(transparentHeatmap, vecData.G, vecData.F);
        Heatmap.HeatLine(transparentHeatmap, vecData.J, vecData.I);
        Heatmap.HeatLine(transparentHeatmap, vecData.F, vecData.J); //  if angle >= 90

        Bitmap heatmap = Heatmap.Apply(basemap, transparentHeatmap, DataSource.mapBounds, space);

        return heatmap;

    }



    public static Pattern DetectPattern(VectorData vectorData)
    {

        //  notes:
        //      -   I may or may not have screwed up how the angles are calculated.
        //          May need to revist it.


        Console.WriteLine("               Trying to detect zone pattern...");


        Pattern pattern = Pattern.none;

        if (IscloseTo(vectorData.pullAngle_low, 0.96 * Math.PI, 0.10))
            pattern = Pattern.floatPull;
        else if (IscloseTo(vectorData.pullAngle_low, 0.62 * Math.PI, 0.10))
            pattern = Pattern.halfMoonDoughnut;
        else if (IscloseTo(vectorData.pullAngle_low, 0.27 * Math.PI, 0.10))
            pattern = Pattern.delayedHardPull;

        Console.WriteLine("               Pattern = " + pattern);

        return pattern;
    }

    private static bool IscloseTo(double value, double desired, double tolerance)
    {
        return Math.Abs(value - desired) <= tolerance;
    }



}