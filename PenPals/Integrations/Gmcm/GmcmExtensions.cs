using GenericModConfigMenu;

namespace PenPals.Integrations.Gmcm;

internal static class GmcmExtensions
{
    public static void AddEnum<T>(
        this IGenericModConfigMenuApi gmcm,
        IManifest mod,
        Func<T> getValue,
        Action<T> setValue,
        Func<string> name,
        Func<string>? tooltip = null
    )
        where T : struct, Enum
    {
        var enumName = typeof(T).Name;
        var allowedValues = Enum.GetNames<T>();
        gmcm.AddTextOption(
            mod,
            getValue: () => getValue().ToString(),
            setValue: valueName => setValue(Enum.Parse<T>(valueName)),
            name,
            tooltip,
            allowedValues,
            formatAllowedValue: value => I18n.GetByKey($"Enum.{enumName}.{value}")
        );
    }
}
