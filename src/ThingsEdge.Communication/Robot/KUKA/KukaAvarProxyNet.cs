using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Robot.KUKA;

/// <summary>
/// Kuka机器人的数据交互类，通讯支持的条件为KUKA 的 KRC4 控制器中运行KUKAVARPROXY 这个第三方软件，端口通常为7000。
/// </summary>
/// <remarks>
/// 非常感谢 昆山-LT 网友的测试和意见反馈。<br />
/// 其中KUKAVARPROXY 这个第三方软件在来源地址：
/// https://github.com/ImtsSrl/KUKAVARPROXY <br />
/// </remarks>
public class KukaAvarProxyNet : NetworkDoubleBase, IRobotNet
{
    private SoftIncrementCount softIncrementCount;

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public KukaAvarProxyNet()
    {
        softIncrementCount = new SoftIncrementCount(65535L, 0L);
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    /// <summary>
    /// 实例化一个默认的Kuka机器人对象，并指定IP地址和端口号，端口号通常为7000。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public KukaAvarProxyNet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new KukaVarProxyMessage();
    }

    /// <summary>
    /// 读取Kuka机器人的数据内容，根据输入的变量名称来读取。
    /// </summary>
    /// <param name="address">地址数据</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string address)
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(PackCommand(BuildReadValueCommand(address))).ConfigureAwait(false), ExtractActualData);
    }

    /// <summary>
    /// 读取Kuka机器人的所有的数据信息，返回字符串信息，解码方式为ANSI，需要指定变量名称。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <returns>带有成功标识的字符串数据</returns>
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return ByteTransformHelper.GetSuccessResultFromOther(await ReadAsync(address).ConfigureAwait(false), Encoding.Default.GetString);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入原始的数据内容。
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">原始的字节数据信息</param>
    /// <returns>是否成功的写入</returns>
    public async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteAsync(address, Encoding.Default.GetString(value)).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据Kuka机器人的变量名称，写入ANSI编码的字符串数据信息。
    /// </summary>
    /// <param name="address">变量名称</param>
    /// <param name="value">ANSI编码的字符串</param>
    /// <returns>是否成功的写入</returns>
    public async Task<OperateResult> WriteAsync(string address, string value)
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(PackCommand(BuildWriteValueCommand(address, value))).ConfigureAwait(false), ExtractActualData);
    }

    /// <summary>
    /// 将核心的指令打包成一个可用于发送的消息对象<br />
    /// Package the core instructions into a message object that can be sent
    /// </summary>
    /// <param name="commandCore">核心命令</param>
    /// <returns>最终实现的可以发送的机器人的字节数据</returns>
    private byte[] PackCommand(byte[] commandCore)
    {
        var array = new byte[commandCore.Length + 4];
        ByteTransform.TransByte((ushort)softIncrementCount.GetCurrentValue()).CopyTo(array, 0);
        ByteTransform.TransByte((ushort)commandCore.Length).CopyTo(array, 2);
        commandCore.CopyTo(array, 4);
        return array;
    }

    private OperateResult<byte[]> ExtractActualData(byte[] response)
    {
        try
        {
            if (response[^1] != 1)
            {
                return new OperateResult<byte[]>(response[^1], "Wrong: " + SoftBasic.ByteToHexString(response, ' '));
            }
            var num = response[5] * 256 + response[6];
            var array = new byte[num];
            Array.Copy(response, 7, array, 0, num);
            return OperateResult.CreateSuccessResult(array);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Wrong:" + ex.Message + " Code:" + SoftBasic.ByteToHexString(response, ' '));
        }
    }

    private byte[] BuildCommands(byte function, string[] commands)
    {
        var list = new List<byte>
        {
            function
        };
        for (var i = 0; i < commands.Length; i++)
        {
            var bytes = Encoding.Default.GetBytes(commands[i]);
            list.AddRange(ByteTransform.TransByte((ushort)bytes.Length));
            list.AddRange(bytes);
        }
        return [.. list];
    }

    private byte[] BuildReadValueCommand(string address)
    {
        return BuildCommands(0, [address]);
    }

    private byte[] BuildWriteValueCommand(string address, string value)
    {
        return BuildCommands(1, [address, value]);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"KukaAvarProxyNet Robot[{IpAddress}:{Port}]";
    }
}
