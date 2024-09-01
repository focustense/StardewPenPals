using GiftMailer.Data;
using Microsoft.Xna.Framework;
using StardewUI;

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
        int giftTaste = who.ActiveItem is not null
            ? npc.getGiftTasteForThisItem(who.ActiveItem)
            : -1;
        var panel = new Panel()
        {
            Name = $"{npc.Name}_Panel",
            Layout = LayoutParameters.Fill(),
            Margin = new(8, 8, 8, 5),
            VerticalContentAlignment = Alignment.End,
            Tooltip = npc.displayName,
            Tags = Tags.Create(npc.Name),
            IsFocusable = true,
            Children = [portrait],
        };
        var (giftTasteSprite, giftTasteTint) = giftTaste switch
        {
            0 => (Sprites.EmojiGrin, Color.Cyan),
            2 => (Sprites.EmojiHappy, Color.White),
            4 => (Sprites.EmojiUnhappy, Color.Orange),
            6 => (Sprites.EmojiAngry, Color.Red),
            _ => (null, Color.White),
        };
        if (giftTasteSprite is not null)
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
                            Sprite = giftTasteSprite,
                            Tint = giftTasteTint,
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
            Layout = LayoutParameters.Fill(),
            Margin = new(16),
            HorizontalAlignment = Alignment.Middle,
            VerticalAlignment = Alignment.Middle,
            Sprite = item is not null ? Sprites.Item(item) : null,
            Tooltip = item?.DisplayName ?? "",
        };
        var frame = new Frame()
        {
            Name = "ItemSelectorFrame",
            Layout = LayoutParameters.FixedSize(96, 96),
            Background = Sprites.ControlBorder,
            VerticalContentAlignment = Alignment.Middle,
            Content = itemImage,
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
}
