namespace ThingsEdge.Contracts.Variables;

/// <summary>
/// 标记数据类型。
/// </summary>
public enum TagDataType
{
    /// <summary>
    /// Bool 类型 <see cref="bool"/>
    /// </summary>
    Bit = 1,

    /// <summary>
    /// 字节 <see cref="byte"/>
    /// </summary>
    Byte,

    /// <summary>
    /// 字 <see cref="ushort"/>
    /// </summary>
    Word,

    /// <summary>
    /// 双字 <see cref="uint"/>
    /// </summary>
    DWord,

    /// <summary>
    /// 短整型 <see cref="short"/>
    /// </summary>
    Int,

    /// <summary>
    /// 长整型 <see cref="int"/>
    /// </summary>
    DInt,

    /// <summary>
    /// 单精度浮点型 <see cref="float"/>
    /// </summary>
    Real,

    /// <summary>
    /// 双精度浮点型 <see cref="double"/>
    /// </summary>
    LReal,

    /// <summary>
    /// 字符串 <see cref="string"/>
    /// </summary>
    String,

    /// <summary>
    /// 西门子 S7String
    /// </summary>
    S7String,

    /// <summary>
    /// 西门子 S7WString
    /// </summary>
    S7WString,
}
