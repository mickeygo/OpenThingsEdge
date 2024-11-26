namespace ThingsEdge.Exchange.Contracts.Variables;

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
    /// 字节（8 位）<see cref="byte"/>
    /// </summary>
    Byte,

    /// <summary>
    /// 字（无符号 16 位）<see cref="ushort"/>
    /// </summary>
    Word,

    /// <summary>
    /// 短整型（带符号 16 位）<see cref="short"/>
    /// </summary>
    Int,

    /// <summary>
    /// 双字（无符号 32 位）<see cref="uint"/>
    /// </summary>
    DWord,

    /// <summary>
    /// 双整型（带符号 32 位）<see cref="int"/>
    /// </summary>
    DInt,

    /// <summary>
    /// 单精度浮点型（32 位）<see cref="float"/>
    /// </summary>
    Real,

    /// <summary>
    /// 长整型（带符号 64 位）<see cref="long"/>
    /// </summary>
    /// <remarks>
    /// Siemens 中没有此类型，在 Omron 等系列中有此类型。
    /// </remarks>
    LInt,

    /// <summary>
    /// 双精度浮点型（64 位）<see cref="double"/>
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
