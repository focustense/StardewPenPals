using Microsoft.Xna.Framework.Graphics;
using StardewUI;
using StardewValley.ItemTypeDefinitions;

namespace PenPals.UI;

/// <summary>
/// Sprites used in the mod UI.
/// </summary>
internal class Sprites
{
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
    /// Frame used to show NPC portraits, e.g. in dialogue. Excludes the name/background portion.
    /// </summary>
    public static Sprite PortraitFrame =>
        new(Game1.mouseCursors, SourceRect: new(603, 414, 74, 74), FixedEdges: new(5));

    /// <summary>
    /// Sprite for the gold star indicating item quality 2.
    /// </summary>
    public static Sprite QualityStarGold =>
        new(Game1.mouseCursors, SourceRect: new(346, 400, 8, 8));

    /// <summary>
    /// Sprite for the iridium star indicating item quality 3.
    /// </summary>
    public static Sprite QualityStarIridium =>
        new(Game1.mouseCursors, SourceRect: new(346, 392, 8, 8));

    /// <summary>
    /// Sprite for the silver star indicating item quality 1.
    /// </summary>
    public static Sprite QualityStarSilver =>
        new(Game1.mouseCursors, SourceRect: new(338, 400, 8, 8));

    /// <summary>
    /// Caret-style arrow pointing right.
    /// </summary>
    public static Sprite RightCaret => new(Game1.mouseCursors, SourceRect: new(448, 96, 24, 32));

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
