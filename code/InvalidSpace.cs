#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

//  todo: utilize the banned zones data in the CanEnd function

class InvalidSpace
{


    
    const string we_elevationPath = $"{DataSource.folder_Space}WE/Space_Elevation.png";
    const string we_hazardPath = $"{DataSource.folder_Space}WE/Space_Hazard.png";
    const string we_partialOOBPath = $"{DataSource.folder_Space}WE/Space_PartialOOB.png";
    const string we_crowdedPath = $"{DataSource.folder_Cache}WE_Space_Crowded.png";
    const string we_outsidePath = $"{DataSource.folder_Space}WE/Space_Outside_Handmade.png";

    const string sp_elevationPath = $"{DataSource.folder_Space}SP/Space_Elevation.png";
    const string sp_hazardPath = $"{DataSource.folder_Space}SP/Space_Hazard.png";
    const string sp_partialOOBPath = $"{DataSource.folder_Space}SP/Space_PartialOOB.png";
    const string sp_crowdedPath = $"{DataSource.folder_Cache}SP_Space_Crowded.png";
    const string sp_outsidePath = $"{DataSource.folder_Space}SP/Space_Outside_Handmade.png";


    //const string bannedPath = $"{DataSource.folder_Space}Banned.png";

    readonly Bitmap elevation;
    static readonly Color elevationColor = Color.FromArgb(48, 83, 183);

    readonly Bitmap hazard;
    static readonly Color hazardColor = Color.FromArgb(158, 28, 55);

    readonly Bitmap partialOOB;
    static readonly Color partialOOBColor = Color.FromArgb(85, 32, 115);

    readonly Bitmap outside;
    static readonly Color outsideColor = Color.FromArgb(157, 233, 74);

    readonly Bitmap banned;
    static readonly Color bannedColor = Color.FromArgb(Color.Red.R, Color.Red.G, Color.Red.B);

    readonly Bitmap crowded;
    static readonly Color crowdedColor = Color.FromArgb(Color.GreenYellow.R, Color.GreenYellow.G, Color.GreenYellow.B);



    public InvalidSpace(string map)
        : 
        this(
            map == "WE" ? we_hazardPath : sp_hazardPath, 
            map == "WE" ? we_elevationPath : sp_elevationPath, 
            map == "WE" ? we_partialOOBPath : sp_partialOOBPath, 
            map == "WE" ? we_outsidePath : sp_outsidePath,
            DataSource.LoadInvalidZones(map),
            map)
    {}

    private InvalidSpace(string hazardPath, string elevationPath, string partialOOBPath, string outsidePath, Bitmap banned, string map)
        : this(
            new Bitmap(hazardPath), 
            new Bitmap(elevationPath), 
            new Bitmap(partialOOBPath), 
            new Bitmap(outsidePath), 
            banned, 
            map)
    {}

    private InvalidSpace(Bitmap hazard, Bitmap elevation, Bitmap partialOOB, Bitmap outside, Bitmap banned, string map)
    {
        this.hazard = hazard;
        this.elevation = elevation;
        this.partialOOB = partialOOB;
        this.outside = outside;
        this.banned = banned;

        string crowdedPath = map == "WE" ? we_crowdedPath : sp_crowdedPath;

        if (File.Exists(crowdedPath))
        {
            this.crowded = new Bitmap(crowdedPath);
        }
        else
        {

            this.crowded = new Bitmap(DataSource.mapResolution.Width, DataSource.mapResolution.Height);

            for (int y = 0; y < DataSource.mapResolution.Width; y++)
            {
                Console.WriteLine($"    Y : {y} / {DataSource.mapResolution.Width - 1}");

                for (int x = 0; x < DataSource.mapResolution.Height; x++)
                {

                    if (this.IsCrowded_Expensive(x, y))
                    {
                        this.crowded.SetPixel(x, y, crowdedColor);
                    }

                }

            }

            this.crowded.Save(crowdedPath, ImageFormat.Png);
        }

    }


    
    public bool IsPlayable(Point p)
    {
        return this.IsPlayable(p.X, p.Y);
    }
    
    public bool IsPlayable(int x, int y)
    {
        if (hazard.GetPixel(x, y) == hazardColor)
            return false;
        if (elevation.GetPixel(x, y) == elevationColor)
            return false;
        if (IsOutside(x, y))
            return false;


        return true;
    }

    public bool CanEnd(Point p)
    {
        return this.CanEnd(p.X, p.Y);
    }

    public bool CanEnd(int x, int y)
    {
        if (!IsPlayable(x, y))
            return false;
        if (banned.GetPixel(x, y) == bannedColor)
            return false;
        if (partialOOB.GetPixel(x, y) == partialOOBColor)
            return false;
        if (IsCrowded(x, y))
            return false;

        return true;
    }

    public bool IsOutside(int x, int y)
    {
        return this.outside.GetPixel(x, y) == outsideColor;
    }

    public bool IsCrowded(int x, int y)
    {
        return this.crowded.GetPixel(x, y) == crowdedColor;
    }

    private bool IsCrowded_Expensive(int x, int y)
    {

        if (IsOutside(x, y))  //  maybe chage this? changed it!, however its way more exspansive now
            return true;
                    
        double radius = DataSource.we_ringRadius5;

        Rectangle squareArea = new Rectangle((int)(x - radius), (int)(y - radius), (int)(radius * 2), (int)(radius * 2));
        int validTiles = 0;
        int inValidTiles = 0;


        for (int yt = squareArea.Y; yt <= squareArea.Bottom; yt++)
        for (int xt = squareArea.X; xt <= squareArea.Right; xt++)
        {
            if (
                (xt >= 0 & xt < DataSource.mapResolution.Width)  &
                (yt >= 0 & yt < DataSource.mapResolution.Height)
            )
            {
                int dx = xt - x, dy = yt - y;

                if (dx * dx + dy * dy <= radius * radius)
                {
                    if (IsPlayable(xt, yt))
                    {
                        validTiles++;
                    }
                    else
                    {
                        inValidTiles++;
                    }

                }
            }
            else
            {
                inValidTiles++;
            }


        }

        return validTiles < inValidTiles;        
    }


    //  checks if the final ring is valid.
    //      it will be valid if the center of the ring is not a banned endzone
    //      and if the majority of the ring is playable
    public bool IsValidCircle(Circle finalRing)
    {
        return this.IsValidCircle(finalRing.X, finalRing.Y);
    }
    public bool IsValidCircle(int x, int y)
    {

        if (!CanEnd(x, y))
            return false;

        return !this.IsCrowded(x, y);
    }

    public void DrawCombined(Bitmap canvas)
    {
        for (int y = 0; y < canvas.Height; y++)
        for (int x = 0; x < canvas.Width; x++)
        {
            if (IsPlayable(x, y))
            {
                if (IsCrowded(x, y))
                    canvas.SetPixel(x, y, crowdedColor);
                else if (!CanEnd(x, y))
                    canvas.SetPixel(x, y, Color.Red);
                
            }
            else
            {
                canvas.SetPixel(x, y, Color.Blue);
            }
        }

    }

    

}