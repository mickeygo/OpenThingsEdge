using System.Net;
using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 基于网络通信的管道信息，可以设置额外的一些参数信息，例如连接超时时间，读取超时时间等等。
/// </summary>
public class PipeSocket : PipeBase, IDisposable
{
    private string _ipAddress = "127.0.0.1";

    private int[] _port = [2000];

    private int _indexPort = -1;

    public IPEndPoint LocalBinding { get; set; }

    public string IpAddress
    {
        get => _ipAddress;
        set => _ipAddress = CommunicationHelper.GetIpAddressFromInput(value);
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.Port" />
    public int Port
    {
        get
        {
            if (_port.Length == 1)
            {
                return _port[0];
            }
            var num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            return _port[num];
        }
        set
        {
            if (_port.Length == 1)
            {
                _port[0] = value;
                return;
            }
            var num = _indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            _port[num] = value;
        }
    }

    /// <summary>
    /// 指示长连接的套接字是否处于错误的状态。
    /// </summary>
    public bool IsSocketError { get; set; }

    /// <summary>
    /// 获取或设置当前的客户端用于服务器连接的套接字。
    /// </summary>
    public Socket Socket { get; set; }

    public int ConnectTimeOut { get; set; } = 10_000;

    public int ReceiveTimeOut { get; set; } = 5_000;

    public int SleepTime { get; set; }

    /// <summary>
    /// 通过指定的IP地址和端口号来实例化一个对象。
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号</param>
    public PipeSocket(string ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = [port];
    }

    /// <summary>
    /// 获取当前的连接状态是否发生了异常，如果发生了异常，返回 False。
    /// </summary>
    /// <returns>如果有异常，返回 True, 否则返回 False</returns>
    public bool IsConnectitonError()
    {
        return IsSocketError || Socket == null;
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public override void Dispose()
    {
        base.Dispose();
        Socket?.Close();
    }

    /// <summary>
    /// 设置多个可选的端口号信息，例如在三菱的PLC里，支持配置多个端口号，当一个网络发生异常时，立即切换端口号连接读写，提升系统的稳定性。
    /// </summary>
    /// <param name="ports">端口号数组信息</param>
    public void SetMultiPorts(int[] ports)
    {
        if (ports != null && ports.Length != 0)
        {
            _port = ports;
            _indexPort = -1;
        }
    }

    /// <summary>
    /// 获取当前的远程连接信息，如果端口号设置了可选的数组，那么每次获取对象就会发生端口号切换的操作。
    /// </summary>
    /// <returns>远程连接的对象</returns>
    public IPEndPoint GetConnectIPEndPoint()
    {
        if (_port.Length == 1)
        {
            return new IPEndPoint(IPAddress.Parse(IpAddress), _port[0]);
        }
        ChangePorts();
        var port = _port[_indexPort];
        return new IPEndPoint(IPAddress.Parse(IpAddress), port);
    }

    /// <summary>
    /// 变更当前的端口号信息，如果设置了多个端口号的话，就切换其他可用的端口。
    /// </summary>
    public void ChangePorts()
    {
        if (_port.Length != 1)
        {
            if (_indexPort < _port.Length - 1)
            {
                _indexPort++;
            }
            else
            {
                _indexPort = 0;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeSocket[{_ipAddress}:{Port}]";
    }
}
