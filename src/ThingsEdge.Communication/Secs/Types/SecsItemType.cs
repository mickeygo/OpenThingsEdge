namespace ThingsEdge.Communication.Secs.Types;

/// <summary>
/// 数据类型的定义
/// </summary>
public enum SecsItemType
{
    /// <summary>
    /// 列表数据类型，代号：L
    /// </summary>
    List,

    /// <summary>
    /// Bool值类型，代号：BOOLEAN
    /// </summary>
    Bool,

    /// <summary>
    /// 二进制数据，代号：B
    /// </summary>
    Binary,

    /// <summary>
    /// ASCII编码的字符串，代号：A
    /// </summary>
    ASCII,

    /// <summary>
    /// JIS类型，代号：J
    /// </summary>
    JIS8,

    /// <summary>
    /// 一个字节的有符号数据，代号：I1
    /// </summary>
    SByte,

    /// <summary>
    /// 一个字节的无符号数据，代号：U1
    /// </summary>
    Byte,

    /// <summary>
    /// 两个字节的有符号数据，代号：I2
    /// </summary>
    Int16,

    /// <summary>
    /// 两个字节的无符号数据，代号：U2
    /// </summary>
    UInt16,

    /// <summary>
    /// 四个字节的有符号数据，代号：I4
    /// </summary>
    Int32,

    /// <summary>
    /// 四个字节的无符号数据，代号：U4
    /// </summary>
    UInt32,

    /// <summary>
    /// 八个字节的有符号数据，代号：I8
    /// </summary>
    Int64,

    /// <summary>
    /// 八个字节的无符号数据，代号：U8
    /// </summary>
    UInt64,

    /// <summary>
    /// 四个字节的浮点数，代号：F4
    /// </summary>
    Single,

    /// <summary>
    /// 八个字节的浮点数，代号：F8
    /// </summary>
    Double,

    /// <summary>
    /// 这是一个空的类型信息
    /// </summary>
    None,
}
