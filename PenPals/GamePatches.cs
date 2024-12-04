namespace PenPals;

internal static class GamePatches
{
    public static bool SuppressQuestSounds { get; set; }

    public static bool PlaySound_Prefix(string cueName)
    {
        return !(SuppressQuestSounds && cueName is "questcomplete");
    }
}
