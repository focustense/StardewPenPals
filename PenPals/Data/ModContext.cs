namespace PenPals.Data;

/// <summary>
/// Context containing the basic top-level dependencies required for the majority of operations.
/// </summary>
/// <param name="modManifest">Manifest data for this mod.</param>
/// <param name="config">Current mod configuration data.</param>
/// <param name="data">Current mod savegame/instance data.</param>
/// <param name="monitor">Monitor for logging.</param>
public class ModContext(IManifest modManifest, ModConfig config, ModData data, IMonitor monitor)
{
    /// <summary>
    /// Current mod configuration data.
    /// </summary>
    public ModConfig Config { get; } = config;

    /// <summary>
    /// Current mod savegame/instance data.
    /// </summary>
    public ModData Data { get; } = data;

    /// <summary>
    /// Manifest data for this mod.
    /// </summary>
    public IManifest ModManifest { get; } = modManifest;

    /// <summary>
    /// Monitor for logging.
    /// </summary>
    public IMonitor Monitor { get; } = monitor;
}
