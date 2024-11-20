namespace ThingsEdge.Communication.Core.IMessage;

/// <summary>
/// 消息类的基类。
/// </summary>
public abstract class NetMessageBase
{
    [NotNull]
    public byte[]? HeadBytes { get; set; }

    [NotNull]
    public byte[]? ContentBytes { get; set; }

    [NotNull]
    public byte[]? SendBytes { get; set; }

    public virtual int PependedUselesByteLength(byte[] headByte)
    {
        return 0;
    }

    public virtual int GetHeadBytesIdentity()
    {
        return 0;
    }

    /// <inheritdoc cref="INetMessage.CheckHeadBytesLegal(byte[])" />
    public virtual bool CheckHeadBytesLegal(byte[] token)
    {
        return true;
    }

    /// <inheritdoc cref="INetMessage.CheckReceiveDataComplete(byte[],MemoryStream)" />
    public virtual bool CheckReceiveDataComplete(byte[] send, MemoryStream ms)
    {
        return true;
    }

    /// <inheritdoc cref="INetMessage.CheckMessageMatch(byte[],byte[])" />
    public virtual int CheckMessageMatch(byte[] send, byte[] receive)
    {
        return 1;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return GetType().Name ?? "";
    }
}
