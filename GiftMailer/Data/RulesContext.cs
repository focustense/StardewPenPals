namespace GiftMailer.Data;

/// <summary>
/// A <see cref="ModContext"/> with additional <see cref="MailRules"/> instance, used for operations
/// dealing specifically with gift prediction/receipt.
/// </summary>
/// <param name="modManifest">Manifest data for this mod.</param>
/// <param name="config">Current mod configuration data.</param>
/// <param name="data">Current mod savegame/instance data.</param>
/// <param name="rules">Rules for sending gifts.</param>
/// <param name="monitor">Monitor for logging.</param>
public class RulesContext(
    IManifest modManifest,
    ModConfig config,
    ModData data,
    MailRules rules,
    IMonitor monitor
) : ModContext(modManifest, config, data, monitor)
{
    /// <summary>
    /// Rules for sending gifts.
    /// </summary>
    public MailRules Rules { get; } = rules;
}
