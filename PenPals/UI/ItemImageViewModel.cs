using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace PenPals.UI;

/// <summary>
/// View model for drawing the image of an in-game item.
/// /// </summary>
/// <remarks>
/// <para>
/// Item images require a specialized context because they are not always simple textures. In many
/// cases, the item must be drawn with a tint or overlay, which is actually a second sprite.
/// </para>
/// <para>
/// Currently does not provide a way to play animations for animated items, like Smoked Fish.
/// </para>
/// </remarks>
/// <param name="sourceTexture">The texture where the item image is located; both the base image and
/// the overlay (if applicable) are expected to be found in this texture.</param>
/// <param name="sourceRect">The region of the <paramref name="sourceTexture"/> where the base image
/// for the item is located.</param>
/// <param name="tintRect">The region of the <paramref name="sourceTexture"/> where the tint or
/// overlay image for the item is located.</paramref>
/// <param name="tintColor">The tint color, which either applies to the image in the
/// <paramref name="tintRect"/>, if one is specified, or the base image (in
/// <paramref name="sourceRect"/>) if no <paramref name="tintRect"/> is specified.</param>
public class ItemImageViewModel(
    Texture2D sourceTexture,
    Rectangle? sourceRect,
    Rectangle? tintRect,
    Color? tintColor
)
{
    /// <summary>
    /// Whether or not the image uses a separate <see cref="TintSprite"/>.
    /// </summary>
    public bool HasTintSprite => TintSprite is not null;

    /// <summary>
    /// Sprite data for the base image.
    /// </summary>
    public Tuple<Texture2D, Rectangle> Sprite { get; } =
        Tuple.Create(sourceTexture, sourceRect ?? sourceTexture.Bounds);

    /// <summary>
    /// Tint color for the <see cref="Sprite"/>.
    /// </summary>
    public Color SpriteColor { get; } = (!tintRect.HasValue ? tintColor : null) ?? Color.White;

    /// <summary>
    /// Sprite data for the overlay image, if any.
    /// </summary>
    public Tuple<Texture2D, Rectangle>? TintSprite { get; } =
        tintRect.HasValue ? Tuple.Create(sourceTexture, tintRect.Value) : null;

    /// <summary>
    /// Tint color for the <see cref="TintSprite"/>.
    /// </summary>
    public Color TintSpriteColor { get; } = tintColor ?? Color.White;

    private static readonly Color smokedFishTintColor = new Color(80, 30, 10) * 0.6f;

    /// <summary>
    /// Creates a new instance using the game data for a known item.
    /// </summary>
    /// <param name="item">The item to display.</param>
    public static ItemImageViewModel ForItem(Item item)
    {
        if (item is SObject obj && obj.preserve.Value == SObject.PreserveType.SmokedFish)
        {
            var fishData = ItemRegistry.GetDataOrErrorItem(obj.GetPreservedItemId());
            return new(
                fishData.GetTexture(),
                fishData.GetSourceRect(),
                fishData.GetSourceRect(),
                smokedFishTintColor
            );
        }
        var data = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
        Color? tintColor = null;
        Rectangle? tintRect = null;
        if (item is ColoredObject co)
        {
            tintColor = co.color.Value;
            if (!co.ColorSameIndexAsParentSheetIndex)
            {
                tintRect = data.GetSourceRect(1);
            }
        }
        return new(data.GetTexture(), data.GetSourceRect(), tintRect, tintColor);
    }
}
