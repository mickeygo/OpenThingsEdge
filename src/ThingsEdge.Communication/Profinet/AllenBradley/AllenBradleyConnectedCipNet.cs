using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Profinet.Omron;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// 基于连接的对象访问的CIP协议的实现，用于对罗克韦尔 PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。
/// </summary>
/// <remarks>
/// 支持普通标签的读写，类型要和标签对应上。如果标签是数组，例如 A 是 INT[0...9] 那么Read("A", 1)，返回的是10个short所有字节数组。
/// 如果需要返回10个长度的short数组，请调用 ReadInt16("A[0], 10"); 地址必须写 "A[0]"，不能写 "A"。
/// </remarks>
public class AllenBradleyConnectedCipNet : OmronConnectedCipNet
{
    /// <summary>
    /// 根据指定的IP及端口来实例化这个连接对象
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口号信息</param>
    public AllenBradleyConnectedCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <inheritdoc />
    protected override int GetMaxTransferBytes()
    {
        return 1980;
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        var read = await ReadAsync(address, length).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        if (read.Content.Length >= 6)
        {
            return OperateResult.CreateSuccessResult(encoding.GetString(count: ByteTransform.TransInt32(read.Content, 2), bytes: read.Content, index: 6));
        }
        return OperateResult.CreateSuccessResult(encoding.GetString(read.Content));
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var data = encoding.GetBytes(value);
        var write = await WriteAsync(address + ".LEN", data.Length).ConfigureAwait(false);
        if (!write.IsSuccess)
        {
            return write;
        }
        return await WriteTagAsync(value: SoftBasic.ArrayExpandToLengthEven(data), address: address + ".DATA[0]", typeCode: 194, length: data.Length).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AllenBradleyConnectedCipNet[{IpAddress}:{Port}]";
    }
}
