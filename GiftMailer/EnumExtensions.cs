using System.Runtime.CompilerServices;
using System.Text;

namespace PenPals;

/// <summary>
/// Extensions for enum and flag types.
/// </summary>
internal static class EnumExtensions
{
    /// <summary>
    /// Gets the localized name for an enum value in the mod's translations.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>Localized name/description for the specified <paramref name="value"/>.</returns>
    public static string ToTranslatedString<T>(this T value)
        where T : Enum
    {
        var enumName = typeof(T).Name;
        return I18n.GetByKey($"Enum.{enumName}.{value}");
    }

    /// <summary>
    /// Gets the sequence of translated strings for all flags included in a
    /// <see cref="FlagsAttribute"/> enum.
    /// </summary>
    /// <typeparam name="T">The enum type with <see cref="FlagsAttribute"/>.</typeparam>
    /// <param name="flags">The flag value.</param>
    /// <returns>Sequence of translated strings for each included flag value.</returns>
    public static IEnumerable<string> ToTranslatedStrings<T>(this T flags)
        where T : unmanaged, Enum
    {
        var sb = new StringBuilder();
        foreach (var value in Enum.GetValues<T>())
        {
            var ordinal = value.AsInteger();
            if (ordinal == 0 || ordinal % 2 != 0 || !flags.HasFlag(value))
            {
                continue;
            }
            yield return value.ToTranslatedString();
        }
    }

    private static int AsInteger<T>(this T value)
        where T : unmanaged, Enum
    {
        var enumSize = Unsafe.SizeOf<T>();
        if (enumSize == Unsafe.SizeOf<byte>())
        {
            return Unsafe.As<T, byte>(ref value);
        }
        if (enumSize == Unsafe.SizeOf<short>())
        {
            return Unsafe.As<T, short>(ref value);
        }
        if (enumSize == Unsafe.SizeOf<int>())
        {
            return Unsafe.As<T, int>(ref value);
        }
        throw new ArgumentException($"Type {typeof(T).Name} cannot fit in an integer type.");
    }
}
