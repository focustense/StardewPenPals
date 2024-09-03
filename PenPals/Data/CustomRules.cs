namespace PenPals.Data;

/// <summary>
/// Gift rules that can be customized in the <c>rules.json</c> or patched by other mods.
/// </summary>
public class CustomRules
{
    /// <summary>
    /// List of qualified IDs of items that can never be mailed as gifts under any circumstances.
    /// </summary>
    public HashSet<string> Blacklist { get; set; } = [];

    /// <summary>
    /// List of qualified IDs of items that can be gifted regardless of how many gifts an NPC has
    /// already received on that day, in that week, etc.
    /// </summary>
    /// <remarks>
    /// Gifting one of these items also will not count against the gift limit for the following day
    /// or week.
    /// </remarks>
    public HashSet<string> IgnoreLimits { get; set; } = [];
}
