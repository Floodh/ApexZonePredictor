#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;

//  todo: utilize the banned zones data in the CanEnd function

class InvalidSpace
{


    
    const string elevationPath = $"{DataSource.folder_Space}Space_Elevation.png";
    const string hazardPath = $"{DataSource.folder_Space}Space_Hazard.png";
    const string partialOOBPath = $"{DataSource.folder_Space}Space_PartialOOB.png";
    //const string bannedPath = $"{DataSource.folder_Space}Banned.png";

    readonly Bitmap elevation;
    static readonly Color elevationColor = Color.FromArgb(48, 83, 183);

    readonly Bitmap hazard;
    static readonly Color hazardColor = Color.FromArgb(158, 28, 55);

    readonly Bitmap partialOOB;
    static readonly Color partialOOBColor = Color.FromArgb(85, 32, 115);

    readonly Bitmap banned;
    static readonly Color bannedColor = Color.FromArgb(Color.Red.R, Color.Red.G, Color.Red.B);

    public InvalidSpace()
        : this(hazardPath, elevationPath, partialOOBPath, DataSource.LoadInvalidZones())
    {}

    private InvalidSpace(string hazardPath, string elevationPath, string partialOOBPath, Bitmap banned)
        : this(new Bitmap(hazardPath), new Bitmap(elevationPath), new Bitmap(partialOOBPath), banned)
    {}

    private InvalidSpace(Bitmap hazard, Bitmap elevation, Bitmap partialOOB, Bitmap banned)
    {
        this.hazard = hazard;
        this.elevation = elevation;
        this.partialOOB = partialOOB;
        this.banned = banned;
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

        return true;
    }


    //  checks if the final ring is valid.
    //      it will be valid if the center of the ring is not a banned endzone
    //      and if the majority of the ring is playable
    public bool IsValidCircle(Circle finalRing)
    {

        if (!CanEnd(finalRing.X, finalRing.Y))
            return false;


        Rectangle squareArea = new Rectangle((int)(finalRing.x - finalRing.radius), (int)(finalRing.y - finalRing.radius), (int)(finalRing.radius * 2), (int)(finalRing.radius * 2));

        int validTiles = 0;
        int inValidTiles = 0;


        for (int y = squareArea.Y; y <= squareArea.Bottom; y++)
        for (int x = squareArea.X; x <= squareArea.Right; x++)
        {
            int dx = x - finalRing.X, dy = y - finalRing.Y;

            if (dx * dx + dy * dy <= finalRing.radius * finalRing.radius)
            {
                if (IsPlayable(x, y))
                {
                    validTiles++;
                }
                else
                {
                    inValidTiles++;
                }

            }


        }

        return validTiles > inValidTiles;
    }


    public void DrawCombined(Bitmap canvas)
    {
        for (int y = 0; y < canvas.Height; y++)
        for (int x = 0; x < canvas.Width; x++)
        {
            if (IsPlayable(x, y))
            {
                if (CanEnd(x, y))
                {
                    continue;
                }
                else
                {
                    canvas.SetPixel(x, y, Color.Red);
                }
            }
            else
            {
                canvas.SetPixel(x, y, Color.Blue);
            }
        }

    }

    

}