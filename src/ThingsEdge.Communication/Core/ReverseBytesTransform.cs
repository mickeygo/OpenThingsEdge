namespace ThingsEdge.Communication.Core;

/// <summary>
/// 大端顺序的字节的转换类，字节的顺序和C#的原生字节的顺序是完全相反的，高字节在前，低字节在后。
/// </summary>
/// <remarks>
/// 适用西门子PLC的S7协议的数据转换。
/// </remarks>
public sealed class ReverseBytesTransform : RegularByteTransform
{
    /// <inheritdoc />
    public ReverseBytesTransform()
    {
        DataFormat = DataFormat.ABCD;
    }

    /// <inheritdoc />
    public ReverseBytesTransform(DataFormat dataFormat)
        : base(dataFormat)
    {
    }

    /// <inheritdoc />
    public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
    {
        return new ReverseBytesTransform(dataFormat)
        {
            IsStringReverseByteWord = IsStringReverseByteWord
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ReverseBytesTransform[{DataFormat}]";
    }
}
