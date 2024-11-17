namespace ThingsEdge.Communication.Reflection;

/// <summary>
/// 结构体的字节偏移信息定义
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class CommStructAttribute : Attribute
{
    /// <summary>
    /// 字节偏移字节信息，如果是bool，就是位偏移地址，按照位为单位
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 长度信息，如果是普通类型，则表示数组，如果是字符串，则表示字符串占用的最大字节长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 编码信息，如果是字符串类型，则表示字符串的编码信息，可选 ASCII UNICODE UTF8 GB2312 ANSI BIG-UNICODE
    /// </summary>
    public string? Encoding { get; set; }

    /// <summary>
    /// 指定偏移地址来实例化一个对象
    /// </summary>
    /// <param name="index">字节偏移字节信息，如果是bool，就是位偏移地址，按照位为单位</param>
    public CommStructAttribute(int index)
    {
        Index = index;
    }

    /// <summary>
    /// 指定偏移地址和长度信息来实例化一个对象
    /// </summary>
    /// <param name="index">字节偏移字节信息，如果是bool，就是位偏移地址，按照位为单位</param>
    /// <param name="length">长度信息，如果是普通类型，则表示数组，如果是字符串，则表示字符串占用的最大字节长度</param>
    public CommStructAttribute(int index, int length)
        : this(index)
    {
        Length = length;
    }

    /// <summary>
    /// 指定偏移地址，长度信息，编码信息来实例化一个对象，通常应用于字符串数据
    /// </summary>
    /// <param name="index">字节偏移字节信息，如果是bool，就是位偏移地址，按照位为单位</param>
    /// <param name="length">长度信息，如果是普通类型，则表示数组，如果是字符串，则表示字符串占用的最大字节长度</param>
    /// <param name="encoding">编码信息，如果是字符串类型，则表示字符串的编码信息，可选 ASCII UNICODE UTF8 GB2312 ANSI BIG-UNICODE</param>
    public CommStructAttribute(int index, int length, string encoding)
        : this(index, length)
    {
        Encoding = encoding;
    }
}
