#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;


class Result
{

    public double acuracy;
    public Bitmap image;
    public VectorData vectorData;
    public string map, dataSet;
    public int gameId;

    public Result(Bitmap basemap, Bitmap heatmap, VectorData vectorData, string map, string dataSet, int gameId)
    {
        this.vectorData = vectorData;
        this.map = map;
        this.dataSet = dataSet;
        this.gameId = gameId;

        Size resolution = DataSource.mapResolution;

        if (basemap.Size != resolution || heatmap.Size != resolution)
            throw new ArgumentException($"Resolution of bitmaps for result does not match expected values: {basemap.Size} {heatmap.Size}");

        this.image = basemap.Clone(new Rectangle(Point.Empty, resolution), basemap.PixelFormat);


        int hit = 0, miss = 0;

        for (int y = 0; y < resolution.Height; y++)
        for (int x = 0; x < resolution.Width; x++)
        {
            

            Color heatmapPixel = heatmap.GetPixel(x,y);
            if (heatmapPixel != basemap.GetPixel(x, y))
            {
                int dx = vectorData.fifthCircle.X - x;
                int dy = vectorData.fifthCircle.Y - y;

                if ((dx * dx + dy * dy) < DataSource.we_ringRadius5 * DataSource.we_ringRadius5)
                {
                    hit++;
                    this.image.SetPixel(x, y, Color.Green);
                }
                else
                {
                    miss++;
                    this.image.SetPixel(x, y, Color.Red);
                }
            }
        }
        this.acuracy = (double)(hit) / (double)(hit + miss);


        Pen pen = new Pen(Color.White);
        Graphics g = Graphics.FromImage(this.image);

        int ringCount = 0;
        foreach (VecPoint p in vectorData.RingCenters)
        {

            Circle circle = new Circle(p.X, p.Y, DataSource.we_ringRadius[ringCount + 1]);
            g.DrawEllipse(pen, circle.DrawRect);
            ringCount++;

        }

        g.Dispose();
        pen.Dispose();

    }

    public void Save()
    {
        this.image.Save($"{DataSource.folder_Output}/Result_{map}_{gameId}.{dataSet}.png");
        
    }


}