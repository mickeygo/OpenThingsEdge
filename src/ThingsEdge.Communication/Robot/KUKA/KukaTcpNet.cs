using System.Net.Sockets;
using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.HslCommunication;

namespace ThingsEdge.Communication.Robot.KUKA;

/// <summary>
/// Kuka机器人的数据交互类，通讯支持的条件为KUKA 的 TCP通讯
/// </summary>
public class KukaTcpNet : NetworkDoubleBase, IRobotNet
{
    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public KukaTcpNet()
    {
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    /// <summary>
    /// 实例化一个默认的Kuka机器人对象，并指定IP地址和端口号，端口号通常为9999<br />
    /// Instantiate a default Kuka robot object and specify the IP address and port number, usually 9999
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public KukaTcpNet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send, bool hasResponseData = true, bool usePackHeader = true)
    {
        var operateResult = Send(socket, send);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        if (ReceiveTimeOut < 0)
        {
            return OperateResult.CreateSuccessResult(new byte[0]);
        }
        var operateResult2 = Receive(socket, -1, ReceiveTimeOut);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult(operateResult2.Content);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true, bool usePackHeader = true)
    {
        var sendValue = usePackHeader ? PackCommandWithHeader(send) : send;
        var sendResult = await SendAsync(socket, sendValue);
        if (!sendResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(sendResult);
        }
        if (ReceiveTimeOut < 0)
        {
            return OperateResult.CreateSuccessResult(new byte[0]);
        }
        var resultReceive = await ReceiveAsync(socket, -1, ReceiveTimeOut);
        if (!resultReceive.IsSuccess)
        {
            return resultReceive;
        }
        return UnpackResponseContent(sendValue, resultReceive.Content);
    }

    /// <summary>
    /// 读取Kuka机器人的数据内容，根据输入的变量名称来读取<br />
    /// Read the data content of the Kuka robot according to the input variable name
    /// </summary>
    /// <param name="address">地址数据</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    [HslMqttApi(ApiTopic = "ReadRobotByte", Description = "Read the data content of the Kuka robot according to the input variable name")]
    public OperateResult<byte[]> Read(string address)
    {
        return ByteTransformHelper.GetResultFromOther(ReadFromCoreServer(Encoding.UTF8.GetBytes(BuildReadCommands(address))), ExtractActualData);
    }

    /// <summary>
    /// 读取Kuka机器人的所有的数据信息，返回字符串信息，解码方式为UTF8，需要指定变量名称<br />
    /// Read all the data information of the Kuka robot, return the string information, decode by ANSI, need to specify the variable name
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的字符串数据</returns>
    [HslMqttApi(ApiTopic = "ReadRobotString", Description = "Read all the data information of the Kuka robot, return the string information, decode by ANSI, need to specify the variable name")]
    public OperateResult<string> ReadString(string address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(Read(address), Encoding.Default.GetString);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入原始的数据内容<br />
    /// Write the original data content according to the variable name of the Kuka robot
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">原始的字节数据信息</param>
    /// <returns>是否成功的写入</returns>
    [HslMqttApi(ApiTopic = "WriteRobotByte", Description = "Write the original data content according to the variable name of the Kuka robot")]
    public OperateResult Write(string address, byte[] value)
    {
        return Write(address, Encoding.Default.GetString(value));
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入UTF8编码的字符串数据信息<br />
    /// Writes ansi-encoded string data information based on the variable name of the Kuka robot
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    [HslMqttApi(ApiTopic = "WriteRobotString", Description = "Writes ansi-encoded string data information based on the variable name of the Kuka robot")]
    public OperateResult Write(string address, string value)
    {
        return Write(new string[1] { address }, new string[1] { value });
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入多个UTF8编码的字符串数据信息<br />
    /// Write multiple UTF8 encoded string data information according to the variable name of the Kuka robot
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    [HslMqttApi(ApiTopic = "WriteRobotStrings", Description = "Write multiple UTF8 encoded string data information according to the variable name of the Kuka robot")]
    public OperateResult Write(string[] address, string[] value)
    {
        return ReadCmd(BuildWriteCommands(address, value));
    }

    private OperateResult ReadCmd(string cmd)
    {
        var operateResult = ReadFromCoreServer(Encoding.UTF8.GetBytes(cmd));
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var @string = Encoding.UTF8.GetString(operateResult.Content);
        if (@string.Contains("err"))
        {
            return new OperateResult("Result contains err: " + @string);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 启动机器人的指定的程序<br />
    /// Start the specified program of the robot
    /// </summary>
    /// <param name="program">程序的名字</param>
    /// <returns>是否启动成功</returns>
    [HslMqttApi(Description = "Start the specified program of the robot")]
    public OperateResult StartProgram(string program)
    {
        return ReadCmd("03" + program);
    }

    /// <summary>
    /// 复位当前的程序<br />
    /// Reset current program
    /// </summary>
    /// <returns>复位结果</returns>
    [HslMqttApi(Description = "Reset current program")]
    public OperateResult ResetProgram()
    {
        return ReadCmd("0601");
    }

    /// <summary>
    /// 停止当前的程序<br />
    /// Stop current program
    /// </summary>
    /// <returns>复位结果</returns>
    [HslMqttApi(Description = "Stop current program")]
    public OperateResult StopProgram()
    {
        return ReadCmd("0621");
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.Read(System.String)" />
    public async Task<OperateResult<byte[]>> ReadAsync(string address)
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(Encoding.UTF8.GetBytes(BuildReadCommands(address))), ExtractActualData);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.ReadString(System.String)" />
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(await ReadAsync(address), Encoding.Default.GetString);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.Write(System.String,System.Byte[])" />
    public async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteAsync(address, Encoding.Default.GetString(value));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.Write(System.String,System.String)" />
    public async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await WriteAsync(new string[1] { address }, new string[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.Write(System.String[],System.String[])" />
    public async Task<OperateResult> WriteAsync(string[] address, string[] value)
    {
        return await ReadCmdAsync(BuildWriteCommands(address, value));
    }

    private async Task<OperateResult> ReadCmdAsync(string cmd)
    {
        var write = await ReadFromCoreServerAsync(Encoding.UTF8.GetBytes(cmd));
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

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.StartProgram(System.String)" />
    public async Task<OperateResult> StartProgramAsync(string program)
    {
        return await ReadCmdAsync("03" + program);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.ResetProgram" />
    public async Task<OperateResult> ResetProgramAsync()
    {
        return await ReadCmdAsync("0601");
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.KUKA.KukaTcpNet.StopProgram" />
    public async Task<OperateResult> StopProgramAsync()
    {
        return await ReadCmdAsync("0621");
    }

    private OperateResult<byte[]> ExtractActualData(byte[] response)
    {
        return OperateResult.CreateSuccessResult(response);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"KukaTcpNet[{IpAddress}:{Port}]";
    }

    /// <summary>
    /// 构建读取变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>报文内容</returns>
    public static string BuildReadCommands(string[] address)
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
                stringBuilder.Append(",");
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 构建读取变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>报文内容</returns>
    public static string BuildReadCommands(string address)
    {
        return BuildReadCommands(new string[1] { address });
    }

    /// <summary>
    /// 构建写入变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="values">数据信息</param>
    /// <returns>字符串信息</returns>
    public static string BuildWriteCommands(string[] address, string[] values)
    {
        if (address == null || values == null)
        {
            return string.Empty;
        }
        if (address.Length != values.Length)
        {
            throw new Exception(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var stringBuilder = new StringBuilder("01");
        for (var i = 0; i < address.Length; i++)
        {
            stringBuilder.Append(address[i] + "=");
            stringBuilder.Append(values[i] ?? "");
            if (i != address.Length - 1)
            {
                stringBuilder.Append(",");
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 构建写入变量的报文命令
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="value">数据信息</param>
    /// <returns>字符串信息</returns>
    public static string BuildWriteCommands(string address, string value)
    {
        return BuildWriteCommands(new string[1] { address }, new string[1] { value });
    }
}
