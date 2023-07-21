#pragma warning disable CA1416

using System.Drawing;
using System.Drawing.Imaging;


static class Organiser
{


    public static void PremadeRename(int gameId, int zoneId)
    {


        string oldName = $"{DataSource.folder_ZoneData}ZoneData_{gameId}_{zoneId}.png";
        string newName = $"{DataSource.folder_ZoneData}WE/ZoneData_{gameId}_{zoneId}.set0.png";
        File.Move(oldName, newName);

    }

    public static void ModifyBannedZoneData()
    {

        //555 153
        //1578 1176

        string folder = $"{DataSource.folder_Space}SP/";
        Rectangle area = new Rectangle(new Point(555, 153), new Size(1578 - 555 - 1, 1176 - 153 - 1));
        
        Bitmap bmpBase = new Bitmap(folder + "Base.png");
        Bitmap bmpBanned = new Bitmap(folder + "Banned.png");

        Bitmap croppedBase = bmpBase.Clone(area, bmpBase.PixelFormat);
        Bitmap croppedBanned = bmpBanned.Clone(area, bmpBanned.PixelFormat);

        Bitmap newBase = new Bitmap(croppedBase, DataSource.mapResolution);
        Bitmap newBanned = new Bitmap(croppedBanned, DataSource.mapResolution);

        newBase.Save(folder + "NewBase.png");
        newBanned.Save(folder + "NewBanned.png");

    }


    public static void SP_GenUnplayable()
    {
        string folder = $"{DataSource.folder_Space}SP/";
        Bitmap bmp = new Bitmap(folder + "Base.png");
        for (int y = 0; y < bmp.Height; y++)
        for (int x = 0; x < bmp.Width; x++)
        {
            Color pixel = bmp.GetPixel(x, y);

            int value = Math.Abs(pixel.R + pixel.G - pixel.B);
            if (value < 50)
            if (pixel.B > 80)
                bmp.SetPixel(x, y, Color.FromArgb(158, 28, 55));
            
        }

        bmp.Save(folder + "Test.png", ImageFormat.Png);
    }

}