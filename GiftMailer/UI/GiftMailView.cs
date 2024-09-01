using GiftMailer.Data;
using StardewUI;
using StardewValley.Menus;

namespace GiftMailer.UI;

internal class GiftMailView(ModConfig config, GiftMailData data, Farmer who, IMonitor monitor)
    : WrapperView
{
    private const int GUTTER_HEIGHT = 150;
    private const int GUTTER_WIDTH = 200;
    private const int SIDEBAR_MARGIN = 16;
    private const int SIDEBAR_WIDTH = 120;

    protected override IView CreateView()
    {
        var viewportSize = Game1.uiViewport.Size;
        var menuWidth = MathF.Min(
            1150,
            viewportSize.Width - GUTTER_WIDTH * 2 - SIDEBAR_WIDTH - SIDEBAR_MARGIN
        );
        var menuHeight = MathF.Min(720, viewportSize.Height - GUTTER_HEIGHT * 2);
        var npcGrid = CreateNpcGrid();
        var itemSelector = CreateSidebar(menuHeight);
        return new ScrollableFrameView()
        {
            Name = "GiftMailRoot",
            FrameLayout = LayoutParameters.FixedSize(menuWidth, menuHeight),
            Title = I18n.GiftMailMenu_Title(),
            Content = npcGrid,
            Sidebar = itemSelector,
            SidebarWidth = SIDEBAR_WIDTH,
        };
    }

    private IView CreateNpcGrid()
    {
        var cells = Game1
            .characterData.Keys.Select(name => Game1.getCharacterFromName(name))
            .Where(npc =>
                npc is not null && npc.CanReceiveGifts() && who.friendshipData.ContainsKey(npc.Name)
            )
            .Select(CreateNpcGridCell)
            .ToList();
        return new Grid()
        {
            Name = "NpcGrid",
            Layout = LayoutParameters.AutoRow(),
            Padding = new(8),
            ItemLayout = GridItemLayout.Length(Sprites.PortraitFrame.Size.X * 2),
            ItemSpacing = new(16, 16),
            Children = cells,
        };
    }

    private IView CreateNpcGridCell(NPC npc)
    {
        var texture = npc.Portrait;
        var sourceRect = Game1.getSourceRectForStandardTileSheet(texture, 0);
        var portrait = new Image()
        {
            Name = $"{npc.Name}_Portrait",
            Layout = LayoutParameters.Fill(),
            Sprite = new(texture, sourceRect),
            VerticalAlignment = Alignment.End,
        };
        var taste = GetGiftTasteInfo(npc, who.ActiveItem);
        var tooltip = npc.displayName;
        if (taste is not null)
        {
            tooltip += Environment.NewLine + "(" + taste.Description + ")";
        }
        var panel = new Panel()
        {
            Name = $"{npc.Name}_Panel",
            Layout = LayoutParameters.Fill(),
            Margin = new(8, 8, 8, 5),
            VerticalContentAlignment = Alignment.End,
            Tooltip = tooltip,
            Tags = Tags.Create(npc),
            IsFocusable = true,
            Children = [portrait],
        };
        panel.Click += OnNpcClick;
        if (taste is not null)
        {
            panel.Children.Add(
                new Panel()
                {
                    Layout = LayoutParameters.Fill(),
                    Margin = new(Right: 2, Bottom: 2),
                    HorizontalContentAlignment = Alignment.End,
                    VerticalContentAlignment = Alignment.End,
                    Children =
                    [
                        new Image()
                        {
                            Layout = LayoutParameters.FixedSize(27, 27),
                            Sprite = taste.Sprite,
                            Tint = taste.Tint,
                        },
                    ],
                }
            );
        }
        return new Frame()
        {
            Name = $"{npc.Name}_Frame",
            Layout = LayoutParameters.FixedSize(
                Sprites.PortraitFrame.Size.X * 2,
                Sprites.PortraitFrame.Size.Y * 2
            ),
            Background = Sprites.PortraitFrame,
            Content = panel,
        };
    }

    private IView CreateSidebar(float height)
    {
        var item = who.ActiveObject;
        var itemImage = new Image()
        {
            Name = $"ItemImage_{item?.Name}",
            Layout = LayoutParameters.Fill(),
            HorizontalAlignment = Alignment.Middle,
            VerticalAlignment = Alignment.Middle,
            Sprite = item is not null ? Sprites.Item(item) : null,
            Tooltip = item?.DisplayName ?? "",
        };
        var itemImagePanel = new Panel()
        {
            Layout = LayoutParameters.Fill(),
            Margin = new(16),
            HorizontalContentAlignment = Alignment.Start,
            VerticalContentAlignment = Alignment.End,
            Children = [itemImage],
        };
        var qualityStarSprite = item?.Quality switch
        {
            1 => Sprites.QualityStarSilver,
            2 => Sprites.QualityStarGold,
            4 => Sprites.QualityStarIridium,
            _ => null,
        };
        if (qualityStarSprite is not null)
        {
            itemImagePanel.Children.Add(
                new Image()
                {
                    Name = $"QualityStar_{item!.Quality}",
                    Layout = LayoutParameters.FixedSize(24, 24),
                    Margin = new(2, 0),
                    Sprite = qualityStarSprite,
                }
            );
        }
        var frame = new Frame()
        {
            Name = "ItemSelectorFrame",
            Layout = LayoutParameters.FixedSize(96, 96),
            Background = Sprites.ControlBorder,
            VerticalContentAlignment = Alignment.Middle,
            Content = itemImagePanel,
        };
        var arrow = new Image()
        {
            Name = "SidebarArrow",
            Layout = LayoutParameters.FitContent(),
            Margin = new(SIDEBAR_MARGIN / 2, 0),
            Sprite = Sprites.RightCaret,
        };
        return new Lane()
        {
            Name = "GiftMailSidebar",
            Layout = LayoutParameters.FixedSize(SIDEBAR_WIDTH, height),
            Margin = new(Right: SIDEBAR_MARGIN),
            VerticalContentAlignment = Alignment.Middle,
            Children = [frame, arrow],
        };
    }

    private GiftTasteInfo? GetGiftTasteInfo(NPC npc, Item? item)
    {
        if (
            item is null
            || config.GiftTasteVisibility == GiftTasteVisibility.None
            || (
                config.GiftTasteVisibility == GiftTasteVisibility.Known
                && !who.hasGiftTasteBeenRevealed(npc, item.ItemId)
            )
        )
        {
            return null;
        }
        int giftTaste = who.ActiveItem is not null
            ? npc.getGiftTasteForThisItem(who.ActiveItem)
            : -1;
        return GiftTasteInfo.ForGiftTaste(giftTaste);
    }

    private void OnNpcClick(object? sender, ClickEventArgs e)
    {
        if (sender is not IView view)
        {
            monitor.Log(
                $"Invalid click detected; sender is not of type {typeof(IView).Name}.",
                LogLevel.Error
            );
            return;
        }
        var npc = view.Tags.Get<NPC>();
        if (npc is null)
        {
            monitor.Log("Could not obtain NPC info from the clicked view.", LogLevel.Error);
            return;
        }
        var item = who.ActiveItem;
        if (item is null)
        {
            monitor.Log(
                "Unable to send gift; player does not have an active item.",
                LogLevel.Error
            );
            return;
        }
        if (config.RequireConfirmation)
        {
            Game1.activeClickableMenu = new ConfirmationDialog(
                I18n.GiftConfirmation_Message(item.DisplayName, npc.displayName),
                _ => ScheduleSend(npc, item)
            );
        }
        else
        {
            ScheduleSend(npc, item);
        }
    }

    private void ScheduleSend(NPC npc, Item item)
    {
        if (who.ActiveItem != item)
        {
            monitor.Log(
                $"Consistency error; player's {nameof(Farmer.ActiveItem)} no longer matches gifted item.",
                LogLevel.Error
            );
            Game1.showRedMessage(I18n.GiftConfirmation_Error());
            return;
        }
        monitor.Log($"Schedule send of {item.Name} (quality {item.Quality}) to {npc.Name}.", LogLevel.Info);
        Game1.exitActiveMenu();
    }
}
