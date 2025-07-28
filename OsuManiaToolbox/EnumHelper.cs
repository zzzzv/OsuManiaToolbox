using OsuManiaToolbox.Core.Services;
using OsuManiaToolbox.Settings;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace OsuManiaToolbox;

public static class EnumItems
{
    public static IReadOnlyList<T> GetValues<T>() where T : Enum
        => (T[])Enum.GetValues(typeof(T));

    public static IReadOnlyList<LogLevel> LogLevels => GetValues<LogLevel>();

    public static IReadOnlyList<LastPlayedSelection> LastPlayedSelections => GetValues<LastPlayedSelection>();

    public static IReadOnlyList<ModGradeStrategyType> ModGradeStrategyTypes => GetValues<ModGradeStrategyType>();
}

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        if (value is Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
                return attr.Description;
        }

        return value.ToString()!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
