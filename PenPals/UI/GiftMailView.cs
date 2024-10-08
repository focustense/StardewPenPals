﻿using System.Text;
using PenPals.Data;
using StardewUI;
using StardewValley.Menus;

namespace PenPals.UI;

internal class GiftMailView(
    ModConfig config,
    GiftMailData data,
    MailRules rules,
    Farmer who,
    IMonitor monitor
) : WrapperView
{
    private const int GUTTER_HEIGHT = 150;
    private const int GUTTER_WIDTH = 200;
    private const int SIDEBAR_MARGIN = 8;
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
            SidebarWidth = 240,
        };
    }

    private IView CreateNpcGrid()
    {
        var cells = Game1
            .characterData.Keys.Select(name => Game1.getCharacterFromName(name))
            .Where(npc => npc is not null)
            .Select(CreateNpcGridCell)
            .Where(cell => cell is not null)
            .Select(cell => cell!)
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

    private IView? CreateNpcGridCell(NPC npc)
    {
        var nonGiftableReasons = rules.CheckGiftability(who, npc, who.ActiveItem);
        if (
            (
                nonGiftableReasons
                & (NonGiftableReasons.Unmet | NonGiftableReasons.CannotReceiveGifts)
            ) != 0
        )
        {
            // For immersion, we'd rather not show unmet NPCs; and non-giftable NPCs just clutter
            // up the UI with non-actionable stuff.
            return null;
        }
        var texture = npc.Portrait;
        var sourceRect = Game1.getSourceRectForStandardTileSheet(texture, 0);
        var portrait = new Image()
        {
            Name = $"{npc.Name}_Portrait",
            Layout = LayoutParameters.Fill(),
            Sprite = new(texture, sourceRect),
            VerticalAlignment = Alignment.End,
        };
        if (nonGiftableReasons != NonGiftableReasons.None)
        {
            portrait.Tint = new(0.15f, 0.15f, 0.15f, 0.5f);
        }
        var taste = GetGiftTasteInfo(npc, who.ActiveItem);
        var tooltipBuilder = new StringBuilder(npc.displayName);
        if (taste is not null)
        {
            tooltipBuilder
                .AppendLine()
                .Append('(')
                .Append(taste.Description(who.ActiveItem.DisplayName))
                .Append(')');
        }
        if (nonGiftableReasons != 0)
        {
            tooltipBuilder
                .AppendLine()
                .AppendLine()
                .Append(I18n.GiftMailMenu_Tooltip_NonGiftable());
            foreach (var reasonText in nonGiftableReasons.ToTranslatedStrings())
            {
                tooltipBuilder.AppendLine().Append("* ").Append(reasonText);
            }
        }
        var panel = new Panel()
        {
            Name = $"{npc.Name}_Panel",
            Layout = LayoutParameters.Fill(),
            Margin = new(8, 8, 8, 5),
            Tooltip = tooltipBuilder.ToString(),
            Tags = Tags.Create(npc, nonGiftableReasons),
            IsFocusable = true,
            Children = [portrait],
        };
        panel.Click += OnNpcClick;
        Image? tasteImage = null;
        if (taste is not null)
        {
            tasteImage = new Image()
            {
                Layout = LayoutParameters.FixedSize(27, 27),
                Sprite = taste.Sprite,
                Tint = taste.Tint,
            };
            panel.Children.Add(
                new Panel()
                {
                    Layout = LayoutParameters.Fill(),
                    Margin = new(Right: 2, Bottom: 2),
                    HorizontalContentAlignment = Alignment.End,
                    VerticalContentAlignment = Alignment.End,
                    Children = [tasteImage],
                }
            );
        }
        if (data.OutgoingGifts.TryGetValue(npc.Name, out var pendingGift))
        {
            panel.Children.Add(
                new Image()
                {
                    Layout = LayoutParameters.FixedSize(32, 32),
                    Margin = new(Left: 2, Bottom: 2),
                    Sprite = Sprites.Item(pendingGift),
                    ShadowAlpha = 0.25f,
                    ShadowOffset = new(-2, 2),
                }
            );
            if (nonGiftableReasons == 0)
            {
                portrait.Tint = new(Color.DimGray, 0.35f);
                if (tasteImage is not null)
                {
                    tasteImage.Tint = new(Color.DarkGray, 0.7f);
                }
            }
            panel.Tooltip = tooltipBuilder
                .AppendLine()
                .AppendLine()
                .Append(I18n.GiftMailMenu_Tooltip_Pending(pendingGift.DisplayName))
                .ToString();
        }
        return new Frame()
        {
            Name = $"{npc.Name}_Frame",
            Layout = LayoutParameters.FixedSize(
                Sprites.PortraitFrame.Size.X * 2,
                Sprites.PortraitFrame.Size.Y * 2
            ),
            Background = Sprites.PortraitFrame,
            BackgroundTint = pendingGift is not null ? new(0.8f, 1.0f, 0.8f) : Color.White,
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
            Background = UiSprites.ControlBorder,
            VerticalContentAlignment = Alignment.Middle,
            Content = itemImagePanel,
        };
        var arrow = new Image()
        {
            Name = "SidebarArrow",
            Layout = LayoutParameters.FitContent(),
            Margin = new(SIDEBAR_MARGIN, 0),
            Sprite = Sprites.RightCaret,
        };
        return new Lane()
        {
            Name = "GiftMailSidebar",
            Layout = LayoutParameters.FixedSize(SIDEBAR_WIDTH, height),
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
        var nonGiftableReasons = view.Tags.Get<NonGiftableReasons>();
        if (nonGiftableReasons != 0)
        {
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
        Game1.playSound("smallSelect");
        if (config.RequireConfirmation)
        {
            Game1.playSound("breathin");
            var confirmationMessage = data.OutgoingGifts.TryGetValue(npc.Name, out var previousGift)
                ? I18n.GiftConfirmation_Replace(
                    previousGift.DisplayName,
                    item.DisplayName,
                    npc.displayName
                )
                : I18n.GiftConfirmation_New(item.DisplayName, npc.displayName);
            Game1.activeClickableMenu = new ConfirmationDialog(
                confirmationMessage,
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
                $"Couldn't schedule gift: player's {nameof(Farmer.ActiveItem)} no longer matches "
                    + "the gifted item.",
                LogLevel.Error
            );
            Game1.showRedMessage(I18n.Hud_Error_ScheduleGift());
            return;
        }
        if (who.ActiveItem.getOne() is not SObject giftObject)
        {
            monitor.Log(
                $"Couldn't schedule gift: the active item {item.QualifiedItemId} is not an Object type.",
                LogLevel.Error
            );
            Game1.showRedMessage(I18n.Hud_Error_ScheduleGift());
            return;
        }
        who.reduceActiveItemByOne();
        if (data.OutgoingGifts.TryGetValue(npc.Name, out var previousGiftObject))
        {
            who.addItemByMenuIfNecessary(previousGiftObject);
        }
        data.OutgoingGifts[npc.Name] = giftObject;
        Game1.playSound("Ship");
        monitor.Log(
            $"Scheduled send of {item.Name} (quality {item.Quality}) to {npc.Name}.",
            LogLevel.Debug
        );
        Game1.exitActiveMenu();
        Game1.addHUDMessage(
            HUDMessage.ForCornerTextbox(
                I18n.Hud_Confirm_GiftSent(
                    item.DisplayName,
                    npc.displayName,
                    I18n.GetByKey($"Hud.Schedule.{config.Scheduling}")
                )
            )
        );
    }
}
