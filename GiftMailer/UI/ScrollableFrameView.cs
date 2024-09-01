using StardewUI;

namespace GiftMailer.UI;

internal class ScrollableFrameView : WrapperView
{
    public IView? Content
    {
        get => contentContainer?.Content;
        set
        {
            var _ = Root; // Ensure view created
            contentContainer.Content = value;
        }
    }

    public LayoutParameters FrameLayout
    {
        get => contentFrame?.Layout ?? default;
        set
        {
            var _ = Root; // Ensure view created
            contentFrame.Layout = value;
        }
    }

    public IView? Sidebar
    {
        get => sidebarContainer?.Children[0];
        set
        {
            var _ = Root; // Ensure view created
            sidebarContainer.Children = value is not null ? [value] : [];
        }
    }

    public int SidebarWidth
    {
        get => sidebarWidth;
        set
        {
            if (sidebarWidth == value)
            {
                return;
            }
            sidebarWidth = value;
            if (IsViewCreated)
            {
                sidebarContainer.Layout = new()
                {
                    Width = Length.Px(sidebarWidth),
                    Height = Length.Content(),
                };
                scrollbar.Layout = new()
                {
                    Width = Length.Px(sidebarWidth),
                    Height = Length.Stretch(),
                };
            }
        }
    }

    public string Title
    {
        get => banner?.Text ?? "";
        set
        {
            var _ = Root; // Ensure view created
            banner.Text = value;
        }
    }

    private int sidebarWidth;

    // Initialized in CreateView
    private Banner banner = null!;
    private ScrollContainer contentContainer = null!;
    private Frame contentFrame = null!;
    private Scrollbar scrollbar = null!;
    private Panel sidebarContainer = null!;
    private Lane scrollingLayout = null!;

    public override void OnWheel(WheelEventArgs e)
    {
        if (e.Handled || scrollbar.Container is not ScrollContainer container)
        {
            return;
        }
        switch (e.Direction)
        {
            case Direction.North when container.Orientation == Orientation.Vertical:
            case Direction.West when container.Orientation == Orientation.Horizontal:
                e.Handled = container.ScrollBackward();
                break;
            case Direction.South when container.Orientation == Orientation.Vertical:
            case Direction.East when container.Orientation == Orientation.Horizontal:
                e.Handled = container.ScrollForward();
                break;
        }
        if (e.Handled)
        {
            Game1.playSound("shwip");
        }
    }

    protected override IView CreateView()
    {
        banner = new Banner()
        {
            Layout = LayoutParameters.FitContent(),
            Margin = new(Top: -85),
            Padding = new(12),
            Background = Sprites.BannerBackground,
            BackgroundBorderThickness =
                (Sprites.BannerBackground.FixedEdges ?? Edges.NONE)
                * (Sprites.BannerBackground.SliceSettings?.Scale ?? 1),
        };
        contentContainer = new ScrollContainer()
        {
            Name = "ContentScrollContainer",
            Peeking = 16,
            ScrollStep = 64,
            Layout = LayoutParameters.Fill(),
        };
        contentFrame = new Frame()
        {
            Name = "ContentFrame",
            Background = Sprites.MenuBackground,
            Border = Sprites.MenuBorder,
            BorderThickness = Sprites.MenuBorderThickness,
            Margin = new(Top: -20),
            Content = contentContainer,
        };
        sidebarContainer = new Panel()
        {
            Layout = new() { Width = Length.Px(sidebarWidth), Height = Length.Content() },
        };
        scrollbar = new Scrollbar(
            Sprites.SmallUpArrow,
            Sprites.SmallDownArrow,
            Sprites.ScrollBarTrack,
            Sprites.VerticalScrollThumb
        )
        {
            Name = "ContentPageScroll",
            Layout = new() { Width = Length.Px(sidebarWidth), Height = Length.Stretch() },
            Margin = new(Top: 10, Bottom: 20),
        };
        scrollbar.Container = contentContainer;
        scrollingLayout = new Lane()
        {
            Name = "ScrollableFrameScrollingLayout",
            Layout = LayoutParameters.FitContent(),
            Children = [sidebarContainer, contentFrame, scrollbar],
            ZIndex = 1,
        };
        return new Panel()
        {
            Name = "ScrollableFrameContentLayout",
            Layout = LayoutParameters.FitContent(),
            HorizontalContentAlignment = Alignment.Middle,
            Children = [banner, scrollingLayout],
        };
    }
}
