using GiftMailer.Data;
using StardewUI;

namespace GiftMailer.UI;

internal class GiftMailMenu(
    ModConfig config,
    GiftMailData data,
    MailRules rules,
    Farmer who,
    IMonitor monitor
) : ViewMenu<GiftMailView>
{
    protected override GiftMailView CreateView()
    {
        return new(config, data, rules, who, monitor);
    }
}
