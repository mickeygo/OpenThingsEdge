using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ThingsEdge.Communication.Core.Pipe;

/// <summary>
/// 基于SSL/TLS加密的管道信息，内部基于 TCP/IP 通信实现<br />
/// Pipe information based on SSL/TLS encryption, and internal TCP/IP communication
/// </summary>
public class PipeSslNet : PipeTcpNet
{
    private bool isServerMode = true;

    private NetworkStream networkStream;

    private SslStream sslStream = null;

    private X509Certificate certificate = null;

    /// <summary>
    /// 获取或设置当前的证书内容，
    /// </summary>
    public X509Certificate Certificate
    {
        get
        {
            return certificate;
        }
        set
        {
            certificate = value;
        }
    }

    /// <summary>
    /// 获取或设置是否检查远程的证书，默认为不检查<br />
    /// </summary>
    public bool RemoteCertificateCheck { get; set; } = false;


    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    /// <param name="serverMode">是否为服务器模式</param>
    public PipeSslNet(bool serverMode)
    {
        isServerMode = serverMode;
    }

    /// <summary>
    /// 通过指定的IP地址和端口号来实例化一个对象<br />
    /// Instantiate an object with the specified IP address and port number
    /// </summary>
    /// <param name="ipAddress">IP地址信息</param>
    /// <param name="port">端口号</param>
    /// <param name="serverMode">是否为服务器模式</param>
    public PipeSslNet(string ipAddress, int port, bool serverMode)
        : base(ipAddress, port)
    {
        isServerMode = serverMode;
    }

    /// <summary>
    /// 通过指定的套接字及连接的终结点来实例化一个对象<br />
    /// Instantiate an object by specifying the socket and the endpoint of the connection
    /// </summary>
    /// <param name="socket">连接的套接字对象</param>
    /// <param name="iPEndPoint">连接的远程地址信息</param>
    /// <param name="serverMode">是否为服务器模式</param>
    public PipeSslNet(Socket socket, IPEndPoint iPEndPoint, bool serverMode)
        : base(socket, iPEndPoint)
    {
        isServerMode = serverMode;
    }

    /// <summary>
    /// 使用一个证书路径来初始化 SSL/TLS 通信<br />
    /// Use a certificate path to initialize SSL/TLS communication
    /// </summary>
    /// <param name="certificateFile">证书路径</param>
    public void SetCertficate(string certificateFile)
    {
        if (!string.IsNullOrEmpty(certificateFile))
        {
            certificate = X509Certificate.CreateFromCertFile(certificateFile);
        }
    }

    /// <inheritdoc />
    protected override OperateResult OnCommunicationOpen(Socket socket)
    {
        var operateResult = CreateSslStream(socket, createNew: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        return base.OnCommunicationOpen(socket);
    }

    /// <summary>
    /// 当前的管道创建一个 SSL/TLS 加密的流<br />
    /// The current pipeline creates an SSL/TLS encrypted stream
    /// </summary>
    /// <param name="socket">套接字对象</param>
    /// <param name="createNew">是否创建一个新的流</param>
    /// <returns>如果创建成功，返回流对象</returns>
    public OperateResult<SslStream> CreateSslStream(Socket socket, bool createNew = false)
    {
        if (createNew)
        {
            networkStream?.Close();
            sslStream?.Close();
            networkStream = new NetworkStream(socket, ownsSocket: false);
            sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false, ValidateCertificate, null);
            try
            {
                if (isServerMode)
                {
                    sslStream.AuthenticateAsServer(certificate, clientCertificateRequired: false, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, checkCertificateRevocation: true);
                    return OperateResult.CreateSuccessResult(sslStream);
                }
                if (certificate == null)
                {
                    sslStream.AuthenticateAsClient(Host);
                }
                else
                {
                    var clientCertificates = new X509CertificateCollection(new X509Certificate[1] { certificate });
                    sslStream.AuthenticateAsClient(Host, clientCertificates, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, checkCertificateRevocation: false);
                }
                return OperateResult.CreateSuccessResult(sslStream);
            }
            catch (Exception ex)
            {
                return new OperateResult<SslStream>(ex.Message);
            }
        }
        return OperateResult.CreateSuccessResult(sslStream);
    }

    private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }
        return !RemoteCertificateCheck;
    }

    /// <inheritdoc />
    public override OperateResult Send(byte[] data, int offset, int size)
    {
        var operateResult = CreateSslStream(Socket);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        var operateResult2 = NetSupport.SocketSend(operateResult.Content, data, offset, size);
        if (!operateResult2.IsSuccess && operateResult2.ErrorCode == NetSupport.SocketErrorCode)
        {
            CloseCommunication();
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), operateResult2.Message);
        }
        return operateResult2;
    }

    /// <inheritdoc />
    public override OperateResult<int> Receive(byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
    {
        var operateResult = CreateSslStream(Socket);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var operateResult2 = NetSupport.SocketReceive(operateResult.Content, buffer, offset, length, timeOut, reportProgress);
        if (!operateResult2.IsSuccess && operateResult2.ErrorCode == NetSupport.SocketErrorCode)
        {
            CloseCommunication();
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + operateResult2.Message);
        }
        return operateResult2;
    }

    /// <inheritdoc />
    public override async Task<OperateResult> SendAsync(byte[] data, int offset, int size)
    {
        var ssl = CreateSslStream(Socket);
        if (!ssl.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(ssl);
        }
        var send = await NetSupport.SocketSendAsync(ssl.Content, data, offset, size).ConfigureAwait(continueOnCapturedContext: false);
        if (!send.IsSuccess && send.ErrorCode == NetSupport.SocketErrorCode)
        {
            await CloseCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
            return new OperateResult<byte[]>(-IncrConnectErrorCount(), send.Message);
        }
        return send;
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int>> ReceiveAsync(byte[] buffer, int offset, int length, int timeOut = 60000, Action<long, long> reportProgress = null)
    {
        var ssl = CreateSslStream(Socket);
        if (!ssl.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(ssl);
        }
        var receive = await NetSupport.SocketReceiveAsync(ssl.Content, buffer, offset, length, timeOut, reportProgress).ConfigureAwait(continueOnCapturedContext: false);
        if (!receive.IsSuccess && receive.ErrorCode == NetSupport.SocketErrorCode)
        {
            await CloseCommunicationAsync().ConfigureAwait(continueOnCapturedContext: false);
            return new OperateResult<int>(-IncrConnectErrorCount(), "Socket Exception -> " + receive.Message);
        }
        return receive;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"PipeSslNet[{Host}:{Port}]";
    }
}
