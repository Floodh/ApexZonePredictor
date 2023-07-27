#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

class Result
{

    
    public Bitmap image;
    public VectorData vectorData;
    public Method methodUsed;
    public string map, dataSet;
    public int gameId;

    public double 
        precision;
    public int
        hits,
        misses,
        markedArea;
    

    private static List<Result> resultList = new List<Result>();
    private static bool cachedResult = false;

    private static double avgPrecision;
    private static int avgHits;
    private static int avgMisses;
    private static int avgMarkedArea;

    private static double medianPrecision;
    private static int medianHits;
    private static int medianMisses;
    private static int medianMarkedArea;

    public Result(Bitmap basemap, Bitmap heatmap, VectorData vectorData, Method methodUsed, string map, string dataSet, int gameId)
    {
        this.vectorData = vectorData;
        this.methodUsed = methodUsed;
        this.map = map;
        this.dataSet = dataSet;
        this.gameId = gameId;

        Size resolution = DataSource.mapResolution;

        if (basemap.Size != resolution || heatmap.Size != resolution)
            throw new ArgumentException($"Resolution of bitmaps for result does not match expected values: {basemap.Size} {heatmap.Size}");

        this.image = basemap.Clone(new Rectangle(Point.Empty, resolution), basemap.PixelFormat);


        this.hits = 0; 
        this.misses = 0;
        this.markedArea = 0;

        for (int y = 0; y < resolution.Height; y++)
        for (int x = 0; x < resolution.Width; x++)
        {
            

            Color heatmapPixel = heatmap.GetPixel(x,y);
            if (heatmapPixel == Heatmap.heatColor)
            {
                int dx = vectorData.fifthCircle.X - x;
                int dy = vectorData.fifthCircle.Y - y;

                if ((dx * dx + dy * dy) < DataSource.we_ringRadius5 * DataSource.we_ringRadius5)
                {
                    hits++;
                    this.image.SetPixel(x, y, Color.Green);
                }
                else
                {
                    dx = vectorData.fourthCircle.X - x;
                    dy = vectorData.fourthCircle.Y - y;                    
                    if ((dx * dx + dy * dy) < DataSource.we_ringRadius4 * DataSource.we_ringRadius4)
                    {
                        this.image.SetPixel(x, y, Color.LightGreen);
                    }
                    else
                    {
                        this.image.SetPixel(x, y, Color.Red);
                        misses++;
                    }
                    
                }

                markedArea++;
            }
        }
        this.precision = (double)(hits) / (double)(hits + misses);


        Pen pen = new Pen(Color.White);
        Graphics g = Graphics.FromImage(this.image);

        int ringCount = 0;
        foreach (VecPoint p in vectorData.RingCenters)
        {

            Circle circle = new Circle(p.X, p.Y, DataSource.we_ringRadius[ringCount + 1]);
            g.DrawEllipse(pen, circle.DrawRect);
            ringCount++;

            if (ringCount == 2)
                pen.Color = Color.Gray;

        }

        g.Dispose();
        pen.Dispose();


        vectorData.Draw(this.image);


        resultList.Add(this);
        cachedResult = false;
    }

    //  make sure to save it after every single result has been calculated.
    public void Save()
    {


        Bitmap output = this.GetImageWithInfo();
        output.Save($"{DataSource.folder_Output}/Result_{map}_{gameId}.{dataSet}.png");


        //  this is temporary, we should not load from fragments
        for (int i = 0; i < 5; i++)
        {

            Bitmap canvas = new Bitmap(DataSource.folder_Fragments + $"{map}_Edgemap_{this.gameId}_{i}.png");

            Pen pen = new Pen(Color.White);
            Graphics g = Graphics.FromImage(canvas);

            int ringCount = 0;
            foreach (VecPoint p in vectorData.RingCenters)
            {

                Circle circle = new Circle(p.X, p.Y, DataSource.we_ringRadius[ringCount + 1]);
                g.DrawEllipse(pen, circle.DrawRect);
                ringCount++;

            }
            
            g.Dispose();
            pen.Dispose();  

            canvas.Save($"{DataSource.folder_Fragments}/EdgemapInprint_{map}_{gameId}_{i}.{dataSet}.png");   
        }

    }

    private Bitmap GetImageWithInfo()
    {
        if (!cachedResult)
            CalculateMedianAndAvg();    //  this will set the bool to true

        Bitmap infoImage = this.image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
        Rectangle drawArea;
        string drawText;


        drawArea = new Rectangle(12, 12, image.Width - 12, image.Height - 12);
        drawText = 
            $"Map - {map}\n" +
            $"Data Set - {dataSet}\n" +
            $"Game Id - {gameId}"
        ;
        DrawText(infoImage, drawArea, drawText, 12);

        


        return infoImage;

    }


    private static void DrawText(Bitmap canvas, Rectangle drawArea, string text, int fontSize)
    {

        Graphics g = Graphics.FromImage(canvas);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.DrawString(text, new Font("Tahoma", fontSize), Brushes.Coral, drawArea);

        g.Flush();

    }

    public static void CalculateMedianAndAvg()
    {

        //  calculate avg first
        int samples = resultList.Count;

        double 
            total_precision = 0;
        int 
            total_hits = 0,
            total_misses = 0,
            total_markedArea = 0;

        foreach (Result result in resultList)
        {
            total_precision += result.precision;
            total_hits += result.hits;
            total_misses += result.misses;
            total_markedArea += result.markedArea;
        }
        avgPrecision = total_precision / samples;
        avgHits = total_hits / samples;
        avgMisses = total_misses / samples;
        avgMarkedArea = total_markedArea / samples;

        //  median, don't need a faster implementation

        double[] precisionArray = new double[samples];
        int[] hitsArray = new int[samples];
        int[] missesArray = new int[samples];
        int[] markedAreaArray = new int[samples];

        for (int i = 0; i < samples; i++)
        {
            precisionArray[i] = resultList[i].precision;
            hitsArray[i] = resultList[i].hits;
            missesArray[i] = resultList[i].misses;
            markedAreaArray[i] = resultList[i].markedArea;
        }

        Array.Sort(precisionArray);
        Array.Sort(hitsArray);
        Array.Sort(missesArray);
        Array.Sort(markedAreaArray);

        //  could make this more correct buuuuuuuuuuuuut, I'm lazy
        medianPrecision = precisionArray[precisionArray.Length / 2];
        medianHits = hitsArray[hitsArray.Length / 2];
        medianMisses = missesArray[missesArray.Length / 2];
        medianMarkedArea = markedAreaArray[markedAreaArray.Length / 2];

        cachedResult = true;
    }

    public static void FlushStaticResults()
    {
        resultList.Clear();
        cachedResult = false;
    }


}