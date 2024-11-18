using System.Net;
using System.Net.Sockets;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 基于网络通信的管道信息，可以设置额外的一些参数信息，例如连接超时时间，读取超时时间等等。
/// </summary>
public class PipeSocket : PipeBase, IDisposable
{
    private string ipAddress = "127.0.0.1";

    private int[] _port = [2000];

    private int indexPort = -1;

    private Socket socket;

    private int receiveTimeOut = 5000;

    private int connectTimeOut = 10000;

    private int sleepTime = 0;

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.LocalBinding" />
    public IPEndPoint LocalBinding { get; set; }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.IpAddress" />
    public string IpAddress
    {
        get
        {
            return ipAddress;
        }
        set
        {
            ipAddress = CommunicationHelper.GetIpAddressFromInput(value);
        }
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
            var num = indexPort;
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
            var num = indexPort;
            if (num < 0 || num >= _port.Length)
            {
                num = 0;
            }
            _port[num] = value;
        }
    }

    /// <summary>
    /// 指示长连接的套接字是否处于错误的状态<br />
    /// Indicates if the long-connected socket is in the wrong state
    /// </summary>
    public bool IsSocketError { get; set; }

    /// <summary>
    /// 获取或设置当前的客户端用于服务器连接的套接字。<br />
    /// Gets or sets the socket currently used by the client for server connection.
    /// </summary>
    public Socket Socket
    {
        get
        {
            return socket;
        }
        set
        {
            socket = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ReceiveTimeOut" />
    public int ConnectTimeOut
    {
        get
        {
            return connectTimeOut;
        }
        set
        {
            connectTimeOut = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ReceiveTimeOut" />
    public int ReceiveTimeOut
    {
        get
        {
            return receiveTimeOut;
        }
        set
        {
            receiveTimeOut = value;
        }
    }

    /// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.SleepTime" />
    public int SleepTime
    {
        get
        {
            return sleepTime;
        }
        set
        {
            sleepTime = value;
        }
    }

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public PipeSocket()
    {
    }

    /// <summary>
    /// 通过指定的IP地址和端口号来实例化一个对象<br />
    /// Instantiate an object with the specified IP address and port number
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号</param>
    public PipeSocket(string ipAddress, int port)
    {
        this.ipAddress = ipAddress;
        _port = new int[1] { port };
    }

    /// <summary>
    /// 获取当前的连接状态是否发生了异常，如果发生了异常，返回 False<br />
    /// Gets whether an exception has occurred in the current connection state, and returns False if an exception has occurred
    /// </summary>
    /// <returns>如果有异常，返回 True, 否则返回 False</returns>
    public bool IsConnectitonError()
    {
        return IsSocketError || socket == null;
    }

    /// <inheritdoc cref="M:System.IDisposable.Dispose" />
    public override void Dispose()
    {
        base.Dispose();
        socket?.Close();
    }

    /// <summary>
    /// 设置多个可选的端口号信息，例如在三菱的PLC里，支持配置多个端口号，当一个网络发生异常时，立即切换端口号连接读写，提升系统的稳定性<br />
    /// Set multiple optional port number information. For example, in Mitsubishi PLC, it supports to configure multiple port numbers. 
    /// When an abnormality occurs in a network, the port number is immediately switched to connect to read and write to improve the stability of the system.
    /// </summary>
    /// <param name="ports">端口号数组信息</param>
    public void SetMultiPorts(int[] ports)
    {
        if (ports != null && ports.Length != 0)
        {
            _port = ports;
            indexPort = -1;
        }
    }

    /// <summary>
    /// 获取当前的远程连接信息，如果端口号设置了可选的数组，那么每次获取对象就会发生端口号切换的操作。<br />
    /// Get the current remote connection information. If the port number is set to an optional array, the port number switching operation will occur every time the object is obtained.
    /// </summary>
    /// <returns>远程连接的对象</returns>
    public IPEndPoint GetConnectIPEndPoint()
    {
        if (_port.Length == 1)
        {
            return new IPEndPoint(IPAddress.Parse(IpAddress), _port[0]);
        }
        ChangePorts();
        var port = _port[indexPort];
        return new IPEndPoint(IPAddress.Parse(IpAddress), port);
    }

    /// <summary>
    /// 变更当前的端口号信息，如果设置了多个端口号的话，就切换其他可用的端口<br />
    /// Change the current port number information, and if multiple port numbers are set, switch to other available ports
    /// </summary>
    public void ChangePorts()
    {
        if (_port.Length != 1)
        {
            if (indexPort < _port.Length - 1)
            {
                indexPort++;
            }
            else
            {
                indexPort = 0;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeSocket[{ipAddress}:{Port}]";
    }
}
