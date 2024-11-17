using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC的EIP协议的实现，当PLC使用了 QJ71EIP71 模块时就需要使用本类来访问
/// </summary>
public class MelsecCipNet : AllenBradleyNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.#ctor" />
    public MelsecCipNet()
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.#ctor(System.String,System.Int32)" />
    public MelsecCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <summary>
    /// Read data information, data length for read array length information
    /// </summary>
    /// <param name="address">Address format of the node</param>
    /// <param name="length">In the case of arrays, the length of the array </param>
    /// <returns>Result data with result object </returns>
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return Read(new string[1] { address }, new ushort[1] { length });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecCipNet.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await ReadAsync(new string[1] { address }, new ushort[1] { length });
    }
}
