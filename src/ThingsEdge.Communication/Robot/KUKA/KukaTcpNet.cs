using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Exceptions;

namespace ThingsEdge.Communication.Robot.KUKA;

/// <summary>
/// Kuka 机器人的数据交互类，通讯支持的条件为 KUKA 的 TCP 通讯。
/// </summary>
public class KukaTcpNet : NetworkDoubleBase, IRobotNet
{
    /// <summary>
    /// 实例化一个默认的Kuka机器人对象，并指定IP地址和端口号，端口号通常为9999。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public KukaTcpNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackHeader = true)
    {
        var sendValue = usePackHeader ? PackCommandWithHeader(send) : send;
        var sendResult = await SendAsync(socket, sendValue).ConfigureAwait(false);
        if (!sendResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(sendResult);
        }

        if (ReceiveTimeOut < 0)
        {
            return OperateResult.CreateSuccessResult(Array.Empty<byte>());
        }
        var resultReceive = await ReceiveAsync(socket, -1, ReceiveTimeOut).ConfigureAwait(false);
        if (!resultReceive.IsSuccess)
        {
            return resultReceive;
        }
        return UnpackResponseContent(sendValue, resultReceive.Content);
    }

    /// <summary>
    /// 读取Kuka机器人的数据内容，根据输入的变量名称来读取。
    /// </summary>
    /// <param name="address">地址数据</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string address)
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(Encoding.UTF8.GetBytes(BuildReadCommands(address))).ConfigureAwait(false), ExtractActualData);
    }

    /// <summary>
    /// 读取Kuka机器人的所有的数据信息，返回字符串信息，解码方式为UTF8，需要指定变量名称。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的字符串数据</returns>
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(await ReadAsync(address).ConfigureAwait(false), Encoding.Default.GetString);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入UTF8编码的字符串数据信息。
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    public async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteAsync(address, Encoding.Default.GetString(value)).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入UTF8编码的字符串数据信息。
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    public async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await WriteAsync([address], [value]).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入多个UTF8编码的字符串数据信息。
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    public async Task<OperateResult> WriteAsync(string[] address, string[] value)
    {
        return await ReadCmdAsync(BuildWriteCommands(address, value)).ConfigureAwait(false);
    }

    private async Task<OperateResult> ReadCmdAsync(string cmd)
    {
        var write = await ReadFromCoreServerAsync(Encoding.UTF8.GetBytes(cmd)).ConfigureAwait(false);
        if (!write.IsSuccess)
        {
            return write;
        }
        var msg = Encoding.UTF8.GetString(write.Content);
        if (msg.Contains("err"))
        {
            return new OperateResult("Result contains err: " + msg);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 启动机器人的指定的程序。
    /// </summary>
    /// <param name="program">程序的名字</param>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> StartProgramAsync(string program)
    {
        return await ReadCmdAsync("03" + program).ConfigureAwait(false);
    }

    /// <summary>
    /// 复位当前的程序。
    /// </summary>
    /// <returns>复位结果</returns>
    public async Task<OperateResult> ResetProgramAsync()
    {
        return await ReadCmdAsync("0601").ConfigureAwait(false);
    }

    /// <summary>
    /// 停止当前的程序。
    /// </summary>
    /// <returns>停止结果</returns>
    public async Task<OperateResult> StopProgramAsync()
    {
        return await ReadCmdAsync("0621").ConfigureAwait(false);
    }

    private OperateResult<byte[]> ExtractActualData(byte[] response)
    {
        return OperateResult.CreateSuccessResult(response);
    }

    /// <summary>
    /// 构建读取变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>报文内容</returns>
    private static string BuildReadCommands(string[] address)
    {
        if (address == null)
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder("00");
        for (var i = 0; i < address.Length; i++)
        {
            stringBuilder.Append(address[i] ?? "");
            if (i != address.Length - 1)
            {
                stringBuilder.Append(',');
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 构建读取变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>报文内容</returns>
    private static string BuildReadCommands(string address)
    {
        return BuildReadCommands([address]);
    }

    /// <summary>
    /// 构建写入变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据信息</param>
    /// <returns>字符串信息</returns>
    private static string BuildWriteCommands(string[] address, string[] values)
    {
        if (address == null || values == null)
        {
            return string.Empty;
        }
        if (address.Length != values.Length)
        {
            throw new CommunicationException(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var stringBuilder = new StringBuilder("01");
        for (var i = 0; i < address.Length; i++)
        {
            stringBuilder.Append(address[i] + "=");
            stringBuilder.Append(values[i] ?? "");
            if (i != address.Length - 1)
            {
                stringBuilder.Append(',');
            }
        }
        return stringBuilder.ToString();
    }

    public override string ToString()
    {
        return $"KukaTcpNet[{IpAddress}:{Port}]";
    }
}
