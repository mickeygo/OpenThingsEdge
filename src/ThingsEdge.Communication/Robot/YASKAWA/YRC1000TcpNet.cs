using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Robot.YASKAWA.Helper;

namespace ThingsEdge.Communication.Robot.YASKAWA;

/// <summary>
/// 安川机器人的Ethernet 服务器功能对应的客户端通讯类。
/// </summary>
/// <remarks>
/// 要想成功的通信，有两个至关重要的前提：
/// 1. 开启以太网服务器，[系统]-[设定]-[选项功能]-[网络功能设定]启用网络功能；
/// 2. 开启远程的命令，[输入输出]-[模拟输入]-[远程命令选择] 激活远程命令。
/// </remarks>
public class YRC1000TcpNet : NetworkDoubleBase, IRobotNet
{
    /// <summary>
    /// 获取或设置当前的机器人类型，默认为 <see cref="YRCType.YRC1000" />。
    /// </summary>
    public YRCType Type { get; set; } = YRCType.YRC1000;

    /// <summary>
    /// 指定机器人的ip地址及端口号来实例化对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public YRC1000TcpNet(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
        ByteTransform = new RegularByteTransform(DataFormat.CDAB);
    }

    public async Task<OperateResult<byte[]>> ReadAsync(string address)
    {
        var read = await ReadStringAsync(address).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(read.Content));
    }

    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        if (address.Contains('.') || address.Contains(':') || address.Contains(';'))
        {
            var commands = address.Split('.', ':', ';');
            return await ReadByCommandAsync(commands[0], commands[1]).ConfigureAwait(false);
        }
        return await ReadByCommandAsync(address, null).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteAsync(address, Encoding.ASCII.GetString(value)).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await ReadByCommandAsync(address, value).ConfigureAwait(false);
    }

    /// <summary>
    /// before read data , the connection should be Initialized
    /// </summary>
    /// <param name="socket">connected socket</param>
    /// <returns>whether is the Initialization is success.</returns>
    protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
    {
        var read = await ReadFromCoreServerAsync(socket, "CONNECT Robot_access KeepAlive:-1\r\n").ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (read.Content == "OK:YR Information Server(Ver) Keep-Alive:-1.\r\n")
        {
            return OperateResult.CreateSuccessResult();
        }
        if (!read.Content.StartsWith("OK:"))
        {
            return new OperateResult(read.Content);
        }
        _isPersistentConn = false;
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13, 10);
    }

    /// <summary>
    /// Read string value from socket
    /// </summary>
    /// <param name="socket">connected socket</param>
    /// <param name="send">string value</param>
    /// <returns>received string value with is successfully</returns>
    protected async Task<OperateResult<string>> ReadFromCoreServerAsync(Socket socket, string send)
    {
        var read = await ReadFromCoreServerAsync(socket, Encoding.Default.GetBytes(send)).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        return OperateResult.CreateSuccessResult(Encoding.Default.GetString(read.Content));
    }

    /// <summary>
    /// 根据指令来读取设备的信息，如果命令数据为空，则传入null即可，注意，所有的命令不带换行符。
    /// </summary>
    /// <remarks>
    /// 此处举几个例子：
    /// "RALARM", NULL    错误报警代码读取；
    /// "RPOSJ", NULL     关节坐标系的坐标位置读取；
    /// "RJSEQ", NULL     读取当前的程序名，行编号，步编号；
    /// "SAVEV", "7,000" 读取变量数据。字符串变量。
    /// </remarks>
    /// <param name="command">命令的内容</param>
    /// <param name="commandData">命令数据内容</param>
    /// <returns>最终的结果内容，需要对IsSuccess进行验证</returns>
    public async Task<OperateResult<string>> ReadByCommandAsync(string command, string? commandData)
    {
        pipeSocket.PipeLockEnter();
        var resultSocket = await GetAvailableSocketAsync().ConfigureAwait(false);
        if (!resultSocket.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            AlienSession?.Offline();
            pipeSocket.PipeLockLeave();
            return OperateResult.CreateFailedResult<string>(resultSocket);
        }

        var readCommand = await ReadFromCoreServerAsync(send: string.IsNullOrEmpty(commandData) ? "HOSTCTRL_REQUEST " + command + " 0\r\n" : $"HOSTCTRL_REQUEST {command} {commandData.Length + 1}\r\n", socket: resultSocket.Content).ConfigureAwait(false);
        if (!readCommand.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            AlienSession?.Offline();
            pipeSocket.PipeLockLeave();
            return OperateResult.CreateFailedResult<string>(readCommand);
        }

        if (!readCommand.Content.StartsWith("OK:"))
        {
            if (!IsPersistentConn)
            {
                resultSocket.Content?.Close();
            }
            pipeSocket.PipeLockLeave();
            return new OperateResult<string>(readCommand.Content.Remove(readCommand.Content.Length - 2));
        }

        if (!string.IsNullOrEmpty(commandData))
        {
            var send2 = Encoding.ASCII.GetBytes(commandData + "\r");
            var sendResult2 = await SendAsync(resultSocket.Content, send2).ConfigureAwait(false);
            if (!sendResult2.IsSuccess)
            {
                resultSocket.Content?.Close();
                pipeSocket.IsSocketError = true;
                AlienSession?.Offline();
                pipeSocket.PipeLockLeave();
                return OperateResult.CreateFailedResult<string>(sendResult2);
            }
        }

        var resultReceive2 = await ReceiveCommandLineFromSocketAsync(resultSocket.Content, 13, ReceiveTimeOut).ConfigureAwait(false);
        if (!resultReceive2.IsSuccess)
        {
            pipeSocket.IsSocketError = true;
            AlienSession?.Offline();
            pipeSocket.PipeLockLeave();
            return OperateResult.CreateFailedResult<string>(resultReceive2);
        }

        var commandDataReturn = Encoding.ASCII.GetString(resultReceive2.Content);
        if (string.IsNullOrEmpty(commandDataReturn))
        {
            if (!IsPersistentConn)
            {
                resultSocket.Content?.Close();
            }
            pipeSocket.PipeLockLeave();
            return new OperateResult<string>("Return is Null");
        }

        if (commandDataReturn.StartsWith("ERROR:"))
        {
            if (!IsPersistentConn)
            {
                resultSocket.Content?.Close();
            }
            pipeSocket.PipeLockLeave();
            await ReceiveAsync(resultSocket.Content!, 1).ConfigureAwait(false);
            return YRCHelper.ExtraErrorMessage(commandDataReturn);
        }
        if (commandDataReturn.StartsWith("0000\r"))
        {
            if (!IsPersistentConn)
            {
                resultSocket.Content?.Close();
            }
            await ReceiveAsync(resultSocket.Content!, 1).ConfigureAwait(false);
            pipeSocket.PipeLockLeave();
            return OperateResult.CreateSuccessResult("0000");
        }

        if (!IsPersistentConn)
        {
            resultSocket.Content?.Close();
        }
        pipeSocket.PipeLockLeave();
        return OperateResult.CreateSuccessResult(commandDataReturn.Remove(commandDataReturn.Length - 1));
    }

    /// <summary>
    /// 读取机器人的报警信息。
    /// </summary>
    /// <returns>原始的报警信息</returns>
    public async Task<OperateResult<string>> ReadALARMAsync()
    {
        return await ReadByCommandAsync("RALARM", null).ConfigureAwait(false);
    }

    /// <summary>
    /// 关节坐标系的坐标位置读取。
    /// </summary>
    /// <returns>原始的报警信息</returns>
    public async Task<OperateResult<string>> ReadPOSJAsync()
    {
        return await ReadByCommandAsync("RPOSJ", null).ConfigureAwait(false);
    }

    /// <summary>
    /// 指定坐标系的当前值读取。并且可以指定外部轴的有无。
    /// </summary>
    /// <param name="coordinate">指定读取坐标 0:基座坐标，1:机器人坐标，2-65分别表示用户坐标1-64</param>
    /// <param name="hasExteralAxis">外部轴的有/无</param>
    /// <returns>坐标系当前值</returns>
    public async Task<OperateResult<YRCRobotData>> ReadPOSCAsync(int coordinate, bool hasExteralAxis)
    {
        var read = await ReadByCommandAsync("RPOSC", string.Format("{0},{1}", coordinate, hasExteralAxis ? "1" : "0")).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<YRCRobotData>(read);
        }
        return OperateResult.CreateSuccessResult(new YRCRobotData(Type, read.Content));
    }

    /// <summary>
    /// 模式状态，循环状态，动作状态，报警错误状态，伺服状态的读取。
    /// </summary>
    /// <remarks>
    /// [0]: 单步
    /// [1]: 1循环
    /// [2]: 自动连续
    /// [3]: 运行中
    /// [4]: 运转中
    /// [5]: 示教
    /// [6]: 在线
    /// [7]: 命令模式
    /// [9]: 示教编程器HOLD中
    /// [10]: 外部HOLD中
    /// [11]: 命令HOLD中
    /// [12]: 发生警报
    /// [13]: 发生错误
    /// [14]: 伺服ON
    /// </remarks>
    /// <returns>状态信息</returns>
    public async Task<OperateResult<bool[]>> ReadStatsAsync()
    {
        var read = await ReadByCommandAsync("RSTATS", null).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToStringArray<byte>().ToBoolArray());
    }

    /// <summary>
    /// 读取当前的程序名，行编号，步编号。
    /// </summary>
    /// <returns>读取结果</returns>
    public Task<OperateResult<string>> ReadJSeqAsync()
    {
        return ReadByCommandAsync("RJSEQ", null);
    }

    /// <summary>
    /// 读取指定用户的坐标数据。
    /// </summary>
    /// <param name="frame">用户坐标编号，1-64</param>
    /// <returns>坐标值</returns>
    public Task<OperateResult<string>> ReadUFrameAsync(int frame)
    {
        return ReadByCommandAsync("RUFRAME", $"{frame}");
    }

    /// <summary>
    /// 读取机器人的字节型变量的数据，需要传入变量的编号。
    /// </summary>
    /// <param name="variableAdderss">变量的编号</param>
    /// <returns>包含是否成功的结果数据</returns>
    public Task<OperateResult<string>> ReadByteVariableAsync(string variableAdderss)
    {
        return ReadByCommandAsync("SAVEV", "0," + variableAdderss);
    }

    /// <summary>
    /// 读取机器人的整型变量的数据，需要传入变量的编号。
    /// </summary>
    /// <param name="variableAdderss">变量的编号</param>
    /// <returns>包含是否成功的结果数据</returns>
    public Task<OperateResult<string>> ReadIntegerVariableAsync(string variableAdderss)
    {
        return ReadByCommandAsync("SAVEV", "1," + variableAdderss);
    }

    /// <summary>
    /// 读取机器人的双精度整型变量的数据，需要传入变量的编号。
    /// </summary>
    /// <param name="variableAdderss">变量的编号</param>
    /// <returns>包含是否成功的结果数据</returns>
    public Task<OperateResult<string>> ReadDoubleIntegerVariableAsync(string variableAdderss)
    {
        return ReadByCommandAsync("SAVEV", "2," + variableAdderss);
    }

    /// <summary>
    /// 读取机器人的实数变量的数据，需要传入变量的编号。
    /// </summary>
    /// <param name="variableAdderss">变量的编号</param>
    /// <returns>包含是否成功的结果数据</returns>
    public Task<OperateResult<string>> ReadRealVariableAsync(string variableAdderss)
    {
        return ReadByCommandAsync("SAVEV", "3," + variableAdderss);
    }

    /// <summary>
    /// 读取机器人的字符串变量的数据，需要传入变量的编号。
    /// </summary>
    /// <param name="variableAdderss">变量的编号</param>
    /// <returns>包含是否成功的结果数据</returns>
    public Task<OperateResult<string>> ReadStringVariableAsync(string variableAdderss)
    {
        return ReadByCommandAsync("SAVEV", "7," + variableAdderss);
    }

    /// <summary>
    /// 进行HOLD 的 ON/OFF 操作，状态参数 False: OFF操作，True: ON操作。
    /// </summary>
    /// <param name="status">状态参数 False: OFF操作，True: ON操作</param>
    /// <returns>是否成功的HOLD操作</returns>
    public async Task<OperateResult> HoldAsync(bool status)
    {
        return await ReadByCommandAsync("HOLD", status ? "1" : "0").ConfigureAwait(false);
    }

    /// <summary>
    /// 对机械手的报警进行复位。
    /// </summary>
    /// <remarks>
    /// 传输报警仅可在示教编程器上进行复位。
    /// </remarks>
    /// <returns>是否复位成功</returns>
    public async Task<OperateResult> ResetAsync()
    {
        return await ReadByCommandAsync("RESET", null).ConfigureAwait(false);
    }

    /// <summary>
    /// 进行错误取消。
    /// </summary>
    /// <returns>是否取消成功</returns>
    public async Task<OperateResult> CancelAsync()
    {
        return await ReadByCommandAsync("CANCEL", null).ConfigureAwait(false);
    }

    /// <summary>
    /// 选择模式。模式编号为1:示教模式，2:再现模式。
    /// </summary>
    /// <param name="number">模式编号为1:示教模式，2:再现模式</param>
    /// <remarks>
    /// MODE 命令，是在「操作条件」 画面中获得外部模式切换的许可后可以使用。
    /// </remarks>
    /// <returns>模式是否选择成功</returns>
    public async Task<OperateResult> ModeAsync(int number)
    {
        return await ReadByCommandAsync("MODE", number.ToString()).ConfigureAwait(false);
    }

    /// <summary>
    /// 选择循环。循环编号 1:步骤，2:1循环，3:连续自动。
    /// </summary>
    /// <param name="number">循环编号 1:步骤，2:1循环，3:连续自动</param>
    /// <returns>循环是否选择成功</returns>
    public async Task<OperateResult> CycleAsync(int number)
    {
        return await ReadByCommandAsync("CYCLE", number.ToString()).ConfigureAwait(false);
    }

    /// <summary>
    /// 进行伺服电源的ON/OFF操作，状态参数 False: OFF，True: ON。
    /// </summary>
    /// <param name="status">状态参数 False: OFF，True: ON</param>
    /// <remarks>
    /// 通过此命令伺服ON的时候，请连接机器人专用端子台（ MTX） 的外部伺服ON（ EXSVON）信号的29和 30 。</remarks>
    /// <returns>是否伺服电源是否成功</returns>
    public async Task<OperateResult> SvonAsync(bool status)
    {
        return await ReadByCommandAsync("SVON", status ? "1" : "0").ConfigureAwait(false);
    }

    /// <summary>
    /// 设定示教编程器和 I/O的操作信号的联锁。 状态参数 False: OFF，True: ON。
    /// </summary>
    /// <param name="status">状态参数 False: OFF，True: ON</param>
    /// <remarks>
    /// 联锁为ON时，仅可执行以下操作。
    /// <list type="number">
    /// <item>示教编程器的非常停止</item>
    /// <item>Ｉ /O 的模式切换， 外部启动， 外部伺服ON，循环切换， I/O 禁止、 PP/PANEL 禁止、 主程序调出以外的输入信号</item>
    /// </list>
    /// 示教编程器在编辑中或者通过其他的功能访问文件时，不能使用HLOCK.
    /// </remarks>
    /// <returns>是否设定成功</returns>
    public async Task<OperateResult> HLockAsync(bool status)
    {
        return await ReadByCommandAsync("HLOCK", status ? "1" : "0").ConfigureAwait(false);
    }

    /// <summary>
    /// 接受消息数据时， 在YRC1000的示教编程器的远程画面下显示消息若。若不是远程画面时，强制切换到远程画面。显示MDSP命令的消息。
    /// </summary>
    /// <param name="message">显示信息（最大 30byte 字符串）</param>
    /// <returns>是否显示成功</returns>
    public async Task<OperateResult> MSDPAsync(string message)
    {
        return await ReadByCommandAsync("MDSP", message).ConfigureAwait(false);
    }

    /// <summary>
    /// 开始程序。操作时指定程序名时，此程序能附带对应主程序，则从该程序的开头开始执行。如果没有指定，则从前行开始执行。
    /// </summary>
    /// <param name="programName">开始动作程序名称，可以省略</param>
    /// <returns>是否启动成功</returns>
    public async Task<OperateResult> StartAsync(string? programName = null)
    {
        return await ReadByCommandAsync("START", programName).ConfigureAwait(false);
    }

    /// <summary>
    /// 删除指定的程序。指定「*」 时， 删除当前登录的所有程序。指定「 删除程序名称」 时，仅删除指定的程序。
    /// </summary>
    /// <param name="programName">删除的程序名称，如果设置为「*」时，删除当前登录的所有程序。</param>
    /// <returns>是否删除成功</returns>
    public async Task<OperateResult> DeleteAsync(string? programName = null)
    {
        return await ReadByCommandAsync("DELETE", programName).ConfigureAwait(false);
    }

    /// <summary>
    /// 指定的程序设定为主程序。设定主程序的同时执行程序也被设定。
    /// </summary>
    /// <param name="programName">设定的程序名称</param>
    /// <returns>是否设定成功</returns>
    public async Task<OperateResult> SetMJAsync(string? programName = null)
    {
        return await ReadByCommandAsync("SETMJ", programName).ConfigureAwait(false);
    }

    /// <summary>
    /// 设定执行程序的名称和行编号。
    /// </summary>
    /// <param name="programName">设定程序名称</param>
    /// <param name="line">设定行编号（ 0 ～ 9999）</param>
    /// <returns>是否设定成功</returns>
    public async Task<OperateResult> JSeqAsync(string programName, int line)
    {
        return await ReadByCommandAsync("JSEQ", $"{programName},{line}").ConfigureAwait(false);
    }

    /// <summary>
    /// 向指定的坐标系位置进行关节动作。其中没有外部轴的系统， 7-12外部轴的值设定为「0」。
    /// </summary>
    /// <param name="robotData">机器的的数据信息</param>
    /// <remarks>
    /// 其中形态数据由6个bool数组组成，每个bool含义参考参数说明，0表示 <c>False</c>，1表示 <c>True</c>
    /// </remarks>
    /// <returns>是否动作成功</returns>
    public async Task<OperateResult> MoveJAsync(YRCRobotData robotData)
    {
        return await ReadByCommandAsync("MOVJ", robotData.ToWriteString(Type)).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取I/O 信号。 I/O 数据是每8个点输出，所以读出接点数是8的倍数。
    /// </summary>
    /// <param name="address">读出开始点编号</param>
    /// <param name="length">读出的接点数</param>
    /// <returns>读取的结果点位信息</returns>
    public async Task<OperateResult<bool[]>> IOReadAsync(int address, int length)
    {
        var read = await ReadByCommandAsync("IOREAD", $"{address},{length}").ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        var buffer = read.Content.ToStringArray<byte>();
        return OperateResult.CreateSuccessResult(buffer.ToBoolArray());
    }

    /// <summary>
    /// 写入I/O信号状态，写入接点数请指定8的倍数。IO 信号的网络写入仅可是（ #27010 ～ #29567）。
    /// </summary>
    /// <param name="address">写入开始接点编号</param>
    /// <param name="value">写入的bool值，写入接点数请指定8的倍数。</param>
    /// <returns>是否写入成功</returns>
    public async Task<OperateResult> IOWriteAsync(int address, bool[] value)
    {
        if (value == null || value.Length % 8 != 0)
        {
            return new OperateResult("Parameter [value] can't be null or length must be 8 *N");
        }

        var buffer = value.ToByteArray();
        var sb = new StringBuilder($"{address},{value.Length}");
        for (var i = 0; i < buffer.Length; i++)
        {
            sb.Append(',');
            sb.Append(buffer[i]);
        }
        return await ReadByCommandAsync("IOWRITE", sb.ToString()).ConfigureAwait(false);
    }

    public override string ToString()
    {
        return $"YRC1000TcpNet Robot[{IpAddress}:{Port}]";
    }
}
