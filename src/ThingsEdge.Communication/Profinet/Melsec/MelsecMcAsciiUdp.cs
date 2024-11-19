using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ascii通讯。
/// </summary>
public class MelsecMcAsciiUdp : MelsecMcAsciiNet, IReadWriteMc, IReadWriteDevice, IReadWriteNet
{
    public MelsecMcAsciiUdp()
        : this("127.0.0.1", 6000)
    {
    }

    public MelsecMcAsciiUdp(string ipAddress, int port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
        CommunicationPipe = new PipeUdpNet();
        IpAddress = ipAddress;
        Port = port;
    }

    public override string ToString()
    {
        return $"MelsecMcAsciiUdp[{IpAddress}:{Port}]";
    }
}
