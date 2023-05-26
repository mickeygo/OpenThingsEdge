namespace ThingsEdge.ServerApp.Converters;

/// <summary>
/// 枚举类型 <see cref="ThemeType"/> 转换为bool类型。
/// </summary>
internal sealed class EnumToBooleanConverter : IValueConverter
{
    public EnumToBooleanConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");

        if (!Enum.IsDefined(typeof(ThemeType), value))
            throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");

        var enumValue = Enum.Parse(typeof(ThemeType), enumString);

        return enumValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string enumString)
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");

        return Enum.Parse(typeof(ThemeType), enumString);
    }
}
