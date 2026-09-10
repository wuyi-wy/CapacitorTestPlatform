using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CapacitorTestPlatform.UI.Converters;

/// <summary>
/// 布尔值转画笔转换器，true 显示绿色，false 显示红色。
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    /// <summary>
    /// 将布尔值转换为画笔：true 为绿色(#27AE60)，false 为红色(#E74C3C)。
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? new SolidColorBrush(Color.FromRgb(39, 174, 96)) : new SolidColorBrush(Color.FromRgb(231, 76, 60));
    }

    /// <summary>不支持反向转换。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 取反布尔值转换器，true → false，false → true。
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    /// <summary>将布尔值取反。</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? false : true;
    }

    /// <summary>反向转换同样取反。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? false : true;
    }
}

/// <summary>
/// 反向布尔值转可见性转换器，true 为 Collapsed，false 为 Visible。
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 将布尔值转换为可见性：true 为 Collapsed，false 为 Visible。
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>不支持反向转换。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 布尔值转字符串转换器，可配置 true/false 对应的显示文本。
/// </summary>
public class BooleanToStringConverter : IValueConverter
{
    /// <summary>true 时显示的文本，默认为 "True"。</summary>
    public string TrueValue { get; set; } = "True";

    /// <summary>false 时显示的文本，默认为 "False"。</summary>
    public string FalseValue { get; set; } = "False";

    /// <summary>
    /// 将布尔值转换为对应配置的字符串。
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? TrueValue : FalseValue;
    }

    /// <summary>
    /// 将字符串反向转换为布尔值，与 TrueValue 匹配则返回 true。
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == TrueValue;
    }
}

/// <summary>
/// 空值转可见性转换器，null 或空字符串显示为 Collapsed，否则 Visible。
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 将 null 或空字符串转换为 Collapsed，否则返回 Visible。
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>不支持反向转换。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 字符串转画笔转换器，将颜色字符串（如 #FF0000）转换为 SolidColorBrush。
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    /// <summary>
    /// 将颜色字符串转换为 SolidColorBrush，解析失败时返回灰色。
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var colorStr = value?.ToString() ?? "#7F8C8D";
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(colorStr);
            return new SolidColorBrush(color);
        }
        catch
        {
            return new SolidColorBrush(Colors.Gray);
        }
    }

    /// <summary>不支持反向转换。</summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
