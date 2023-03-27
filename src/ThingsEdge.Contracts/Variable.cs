namespace ThingsEdge.Contracts;

/// <summary>
/// 地址变量
/// </summary>
public sealed class Variable : IEquatable<Variable>
{
    /// <summary>
    /// 地址的标签名。
    /// </summary>
    [NotNull]
    public string? Tag { get; set; }

    /// <summary>
    /// 地址 (字符串格式)。
    /// </summary>
    [NotNull]
    public string? Address { get; set; }

    /// <summary>
    /// 变量长度。
    /// </summary>
    /// <remarks>普通类型默认长度设置为 0，当为数组或字符串时，需指定长度。</remarks>
    public int Length { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [NotNull]
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量描述。
    /// </summary>
    [NotNull]
    public string? Desc { get; } = string.Empty;

    #region override

    public bool Equals(Variable? other)
    {
        return other != null &&
            Tag.Equals(other.Tag, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is Variable obj2 && Equals(obj2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Tag);
    }

    #endregion
}
