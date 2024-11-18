using System.Net;
using System.Net.NetworkInformation;
using ThingsEdge.Communication.Core.Pipe;

namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// 基于UDP/IP管道的设备基类信息
/// </summary>
public class DeviceUdpNet : DeviceCommunication
{
    private PipeUdpNet _pipe;

    /// <inheritdoc />
    public CommunicationPipe? CommunicationPipe
    {
        get
        {
            return base.CommunicationPipe;
        }
        set
        {
            base.CommunicationPipe = value;
            if (value is PipeUdpNet pipeUdpNet)
            {
                _pipe = pipeUdpNet;
            }
        }
    }

    /// <inheritdoc cref="P:DeviceTcpNet.IpAddress" />
    public virtual string IpAddress
    {
        get
        {
            return _pipe.IpAddress;
        }
        set
        {
            _pipe.IpAddress = value;
        }
    }

    /// <inheritdoc cref="P:DeviceTcpNet.Port" />
    public virtual int Port
    {
        get
        {
            return _pipe.Port;
        }
        set
        {
            _pipe.Port = value;
        }
    }

    /// <inheritdoc />
    public int ReceiveCacheLength
    {
        get
        {
            return _pipe.ReceiveCacheLength;
        }
        set
        {
            _pipe.ReceiveCacheLength = value;
        }
    }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public DeviceUdpNet()
        : this("127.0.0.1", 5000)
    {
    }

    /// <summary>
    /// 指定IP地址以及端口号信息来初始化对象
    /// </summary>
    /// <param name="ipAddress">IP地址信息，可以是IPv4, IPv6, 也可以是域名</param>
    /// <param name="port">设备方的端口号信息</param>
    public DeviceUdpNet(string ipAddress, int port)
    {
        CommunicationPipe = new PipeUdpNet
        {
            IpAddress = ipAddress,
            Port = port
        };
    }

    /// <inheritdoc cref="M:HslCommunication.Core.Device.DeviceTcpNet.IpAddressPing" />
    public IPStatus IpAddressPing()
    {
        var ping = new Ping();
        return ping.Send(IpAddress).Status;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceUdpNet<{ByteTransform}>{{{CommunicationPipe}}}";
    }
}
