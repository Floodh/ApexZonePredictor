


static class Organiser
{


    public static void PremadeRename(int gameId, int zoneId)
    {


        string oldName = $"{DataSource.folder_ZoneData}ZoneData_{gameId}_{zoneId}.png";
        string newName = $"{DataSource.folder_ZoneData}WE/ZoneData_{gameId}_{zoneId}.set0.png";
        File.Move(oldName, newName);

    }

}