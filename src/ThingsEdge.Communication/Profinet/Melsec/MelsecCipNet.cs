using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱 PLC 的 EIP 协议的实现，当 PLC 使用了 QJ71EIP71 模块时就需要使用本类来访问。
/// </summary>
public class MelsecCipNet : AllenBradleyNet
{
    public MelsecCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <summary>
    /// Read data information, data length for read array length information。
    /// </summary>
    /// <param name="address">Address format of the node</param>
    /// <param name="length">In the case of arrays, the length of the array </param>
    /// <returns>Result data with result object </returns>
    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return ReadAsync([address], [length]);
    }
}
