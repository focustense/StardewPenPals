using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewValley.ItemTypeDefinitions;

namespace GiftMailer.UI;

/// <summary>
/// Sprites used in the mod UI.
/// </summary>
internal class Sprites
{
    /// <summary>
    /// Background for the a banner or "scroll" style text, often used for menu/dialogue titles.
    /// </summary>
    public static Sprite BannerBackground =>
        new(
            Game1.mouseCursors,
            SourceRect: new(325, 318, 25, 18),
            FixedEdges: new(12, 0),
            SliceSettings: new(Scale: 4)
        );

    /// <summary>
    /// Border/background sprite for an individual control, such as a button. Less prominent than
    /// <see cref="MenuBorder"/>.
    /// </summary>
    public static Sprite ControlBorder =>
        new(Game1.menuTexture, SourceRect: new(0, 256, 60, 60), FixedEdges: new(16));

    /// <summary>
    /// Standard "angry" icon from the chatbox emojis.
    /// </summary>
    public static Sprite EmojiAngry => GetEmojiSprite(14);

    /// <summary>
    /// Grin (very happy) icon from the chatbox emojis.
    /// </summary>
    public static Sprite EmojiGrin => GetEmojiSprite(1);

    /// <summary>
    /// Standard "happy" icon from the chatbox emojis.
    /// </summary>
    public static Sprite EmojiHappy => GetEmojiSprite(0);

    /// <summary>
    /// Standard "unhappy" icon from the chatbox emojis.
    /// </summary>
    public static Sprite EmojiUnhappy => GetEmojiSprite(12);

    /// <summary>
    /// Background used for the in-game menu, not including borders.
    /// </summary>
    public static Sprite MenuBackground => new(Game1.menuTexture, SourceRect: new(64, 128, 64, 64));

    /// <summary>
    /// Modified 9-slice sprite used for the menu border, based on menu "tiles". Used for drawing the outer border of an
    /// entire menu UI.
    /// </summary>
    public static Sprite MenuBorder =>
        new(
            Game1.menuTexture,
            SourceRect: new(0, 0, 256, 256),
            FixedEdges: new(64),
            SliceSettings: new(CenterX: 128, CenterY: 128, EdgesOnly: true)
        );

    /// <summary>
    /// The actual distance from the outer edges of the <see cref="MenuBorder"/> sprite to where the actual "border"
    /// really ends, in terms of pixels. The border tiles are quite large, so this tends to be needed in order to
    /// determine where the content should go without adding a ton of extra padding.
    /// </summary>
    public static Edges MenuBorderThickness => new(36, 36, 40, 36);

    /// <summary>
    /// Frame used to show NPC portraits, e.g. in dialogue. Excludes the name/background portion.
    /// </summary>
    public static Sprite PortraitFrame =>
        new(Game1.mouseCursors, SourceRect: new(603, 414, 74, 74), FixedEdges: new(5));

    /// <summary>
    /// Caret-style arrow pointing right.
    /// </summary>
    public static Sprite RightCaret => new(Game1.mouseCursors, SourceRect: new(448, 96, 24, 32));

    /// <summary>
    /// Background for the scroll bar track (which the thumb is inside).
    /// </summary>
    public static Sprite ScrollBarTrack =>
        new(
            Game1.mouseCursors,
            SourceRect: new(403, 383, 6, 6),
            FixedEdges: new(2),
            SliceSettings: new(Scale: 4)
        );

    /// <summary>
    /// Small up arrow, typically used for scroll bars.
    /// </summary>
    public static Sprite SmallUpArrow => new(Game1.mouseCursors, SourceRect: new(421, 459, 11, 12));

    /// <summary>
    /// Small down arrow, typically used for scroll bars.
    /// </summary>
    public static Sprite SmallDownArrow =>
        new(Game1.mouseCursors, SourceRect: new(421, 472, 11, 12));

    /// <summary>
    /// Thumb sprite used for vertical scroll bars.
    /// </summary>
    public static Sprite VerticalScrollThumb =>
        new(Game1.mouseCursors, SourceRect: new(435, 463, 6, 10));

    /// <summary>
    /// Gets a <see cref="Sprite"/> for an in-game item, given its item data.
    /// </summary>
    /// <returns>
    /// The item sprite.
    /// </returns>
    public static Sprite Item(ParsedItemData itemData)
    {
        return new(itemData.GetTexture(), SourceRect: itemData.GetSourceRect());
    }

    /// <summary>
    /// Gets a <see cref="Sprite"/> for an in-game item.
    /// </summary>
    /// <returns>
    /// The item sprite.
    /// </returns>
    public static Sprite Item(Item item)
    {
        var itemData = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
        return Item(itemData);
    }

    private static Sprite GetEmojiSprite(int index)
    {
        const int columns = 14;
        const int size = 9;
        var texture = GetEmojiTexture();
        var top = index / columns * size;
        var left = index % columns * size;
        return new(texture, new(left, top, size, size));
    }

    private static Texture2D GetEmojiTexture()
    {
        return Game1.content.Load<Texture2D>(@"LooseSprites\emojis");
    }
}
