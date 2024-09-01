namespace GiftMailer.Data;

/// <summary>
/// Top-level data for the mod.
/// </summary>
public class ModData
{
    /// <summary>
    /// Gift data per farmer, grouped by <see cref="Farmer.UniqueMultiplayerID"/>.
    /// </summary>
    public Dictionary<long, GiftMailData> FarmerGiftMail { get; set; } = [];
}
