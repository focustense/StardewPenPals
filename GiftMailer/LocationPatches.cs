namespace PenPals;

internal static class LocationPatches
{
    public static bool SuppressGiftSounds { get; set; }

    public static bool LocalSound_Prefix(string audioName)
    {
        return !(SuppressGiftSounds && audioName == "give_gift");
    }
}
