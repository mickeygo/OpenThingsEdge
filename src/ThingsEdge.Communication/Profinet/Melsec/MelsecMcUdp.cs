using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯<br />
/// Mitsubishi PLC communication class is implemented using UDP protocol and Qna compatible 3E frame protocol. 
/// The Ethernet module needs to be configured first on the PLC side, and it must be binary communication.
/// </summary>
/// <remarks>
/// <inheritdoc cref="T:HslCommunication.Profinet.Melsec.MelsecMcNet" path="remarks" />
/// </remarks>
/// <example>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage" title="简单的短连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage2" title="简单的长连接使用" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample1" title="基本的读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample2" title="批量读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
/// </example>
public class MelsecMcUdp : MelsecMcNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.#ctor" />
    public MelsecMcUdp()
        : this("127.0.0.1", 6000)
    {
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecMcNet.#ctor(System.String,System.Int32)" />
    public MelsecMcUdp(string ipAddress, int port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
        CommunicationPipe = new PipeUdpNet();
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecMcUdp[{IpAddress}:{Port}]";
    }
}
