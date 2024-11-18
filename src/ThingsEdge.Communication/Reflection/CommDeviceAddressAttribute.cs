namespace ThingsEdge.Communication.Reflection;

/// <summary>
/// 应用于Comm组件库读取的动态地址解析，具体用法为创建一个类，创建数据属性，如果这个属性需要绑定PLC的真实数据，就在属性的特性上应用本特性。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class CommDeviceAddressAttribute : Attribute
{
    /// <summary>
    /// 设备的类型，如果指定了特殊的PLC，那么该地址就可以支持多种不同PLC。
    /// </summary>
    public Type? DeviceType { get; set; }

    /// <summary>
    /// 数据的地址信息，真实的设备的地址信息。
    /// </summary>
    [NotNull]
    public string? Address { get; }

    /// <summary>
    /// 读取的数据长度。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 如果关联了字符串类型的数据，则表示指定的字符编码，默认 ASCII 编码。
    /// </summary>
    public string Encoding { get; set; } = "ASCII";

    /// <summary>
    /// 实例化一个地址特性，指定地址信息，用于单变量的数据。
    /// </summary>
    /// <param name="address">真实的地址信息</param>
    public CommDeviceAddressAttribute(string address)
    {
        Address = address;
        Length = -1;
        DeviceType = null;
    }

    /// <summary>
    /// 实例化一个地址特性，指定地址信息，用于单变量的数据，并指定设备类型。
    /// </summary>
    /// <param name="address">真实的地址信息</param>
    /// <param name="deviceType">设备的地址信息</param>
    public CommDeviceAddressAttribute(string address, Type deviceType)
    {
        Address = address;
        Length = -1;
        DeviceType = deviceType;
    }

    /// <inheritdoc />
    public CommDeviceAddressAttribute(string address, int length)
    {
        Address = address;
        Length = length;
        DeviceType = null;
    }

    /// <summary>
    /// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取。
    /// </summary>
    /// <param name="address">真实的地址信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="encoding">如果是字符串类型的数据的话，就是字符串编码</param>
    public CommDeviceAddressAttribute(string address, int length, string encoding)
    {
        Address = address;
        Length = length;
        DeviceType = null;
        Encoding = encoding;
    }

    /// <inheritdoc />
    public CommDeviceAddressAttribute(string address, int length, Type deviceType)
    {
        Address = address;
        Length = length;
        DeviceType = deviceType;
    }

    /// <summary>
    /// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取，并指定设备的类型，可用于不同种类的PLC。
    /// </summary>
    /// <param name="address">真实的地址信息</param>
    /// <param name="length">读取的数据长度</param>
    /// <param name="deviceType">设备类型</param>
    /// <param name="encoding">如果是字符串类型的数据的话，就是字符串编码</param>
    public CommDeviceAddressAttribute(string address, int length, Type deviceType, string encoding)
    {
        Address = address;
        Length = length;
        DeviceType = deviceType;
        Encoding = encoding;
    }

    /// <summary>
    /// 获取数据的数量信息，如果小于0，则返回1。
    /// </summary>
    /// <returns>数据的个数</returns>
    public int GetDataLength()
    {
        return Length < 0 ? 1 : Length;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"HslDeviceAddressAttribute[{Address}:{Length}]";
    }

    /// <summary>
    /// 获取当前关联的编码信息，通常用于解析字符串的操作。
    /// </summary>
    /// <returns>字符编码信息</returns>
    public Encoding GetEncoding()
    {
        if (Encoding.Equals("ASCII", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.ASCII;
        }
        if (Encoding.Equals("Unicode", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.Unicode;
        }
        if (Encoding.Equals("BigEndianUnicode", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.BigEndianUnicode;
        }
        if (Encoding.Equals("UTF8", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.UTF8;
        }
        if (Encoding.Equals("ANSI", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.Default;
        }
        if (Encoding.Equals("UTF32", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.UTF32;
        }
        if (Encoding.Equals("GB2312", StringComparison.OrdinalIgnoreCase))
        {
            return System.Text.Encoding.GetEncoding("gb2312");
        }
        return System.Text.Encoding.GetEncoding(Encoding);
    }
}
