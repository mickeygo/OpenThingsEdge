using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯。
/// </summary>
public class MelsecMcUdp : MelsecMcNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    public MelsecMcUdp()
        : this("127.0.0.1", 6000)
    {
    }

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
