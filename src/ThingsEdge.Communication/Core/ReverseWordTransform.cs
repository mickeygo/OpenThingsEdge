namespace ThingsEdge.Communication.Core;

/// <summary>
/// 按照字节错位的数据转换类。
/// </summary>
public class ReverseWordTransform : RegularByteTransform
{
    /// <inheritdoc />
    public ReverseWordTransform()
    {
        DataFormat = DataFormat.CDAB;
    }

    /// <inheritdoc />
    public ReverseWordTransform(DataFormat dataFormat)
        : base(dataFormat)
    {
    }

    /// <inheritdoc />
    public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
    {
        return new ReverseWordTransform(dataFormat)
        {
            IsStringReverseByteWord = IsStringReverseByteWord
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"ReverseWordTransform[{DataFormat}]";
    }
}
