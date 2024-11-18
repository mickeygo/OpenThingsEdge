using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Robot.YAMAHA;

/// <summary>
/// 雅马哈机器人的数据访问类
/// </summary>
public class YamahaRCX : NetworkDoubleBase
{
    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public YamahaRCX()
    {
        ByteTransform = new RegularByteTransform();
        ReceiveTimeOut = 30000;
    }

    /// <summary>
    /// 指定IP地址和端口来实例化一个对象
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="port">端口号</param>
    public YamahaRCX(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13, 10);
    }

    /// <summary>
    /// 发送命令行到socket, 并从机器人读取指定的命令行
    /// </summary>
    /// <param name="send">等待发送的数据</param>
    /// <param name="lines">接收的行数</param>
    /// <returns>结果的结果数据内容</returns>
    public OperateResult<string[]> ReadFromServer(byte[] send, int lines)
    {
        var operateResult = new OperateResult<string[]>();
        OperateResult<Socket> operateResult2 = null;
        pipeSocket.PipeLockEnter();
        try
        {
            operateResult2 = GetAvailableSocket();
            if (!operateResult2.IsSuccess)
            {
                pipeSocket.IsSocketError = true;
                AlienSession?.Offline();
                pipeSocket.PipeLockLeave();
                operateResult.CopyErrorFromOther(operateResult2);
                return operateResult;
            }
            var list = new List<string>();
            var flag = false;
            for (var i = 0; i < lines; i++)
            {
                var operateResult3 = ReadFromCoreServer(operateResult2.Content, send);
                if (!operateResult3.IsSuccess)
                {
                    flag = true;
                    pipeSocket.IsSocketError = true;
                    AlienSession?.Offline();
                    operateResult.CopyErrorFromOther(operateResult3);
                    break;
                }
                list.Add(Encoding.ASCII.GetString(operateResult3.Content.RemoveLast(2)));
            }
            if (!flag)
            {
                pipeSocket.IsSocketError = false;
                operateResult.IsSuccess = true;
                operateResult.Content = list.ToArray();
                operateResult.Message = StringResources.Language.SuccessText;
            }
            ExtraAfterReadFromCoreServer(new OperateResult
            {
                IsSuccess = !flag
            });
            pipeSocket.PipeLockLeave();
        }
        catch
        {
            pipeSocket.PipeLockLeave();
            throw;
        }
        if (!_isPersistentConn)
        {
            operateResult2?.Content?.Close();
        }
        return operateResult;
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadFromServer(System.Byte[],System.Int32)" />
    public async Task<OperateResult<string[]>> ReadFromServerAsync(byte[] send, int lines)
    {
        var result = new OperateResult<string[]>();
        await Task.Run(delegate
        {
            pipeSocket.PipeLockEnter();
        });
        OperateResult<Socket> resultSocket;
        try
        {
            resultSocket = await GetAvailableSocketAsync();
            if (!resultSocket.IsSuccess)
            {
                pipeSocket.IsSocketError = true;
                AlienSession?.Offline();
                pipeSocket.PipeLockLeave();
                result.CopyErrorFromOther(resultSocket);
                return result;
            }
            var buffers = new List<string>();
            var isError = false;
            for (var i = 0; i < lines; i++)
            {
                var read = await ReadFromCoreServerAsync(resultSocket.Content, send);
                if (!read.IsSuccess)
                {
                    isError = true;
                    pipeSocket.IsSocketError = true;
                    AlienSession?.Offline();
                    result.CopyErrorFromOther(read);
                    break;
                }
                buffers.Add(Encoding.ASCII.GetString(read.Content.RemoveLast(2)));
            }
            if (!isError)
            {
                pipeSocket.IsSocketError = false;
                result.IsSuccess = true;
                result.Content = buffers.ToArray();
                result.Message = StringResources.Language.SuccessText;
            }
            ExtraAfterReadFromCoreServer(new OperateResult
            {
                IsSuccess = !isError
            });
            pipeSocket.PipeLockLeave();
        }
        catch
        {
            pipeSocket.PipeLockLeave();
            throw;
        }
        if (!_isPersistentConn)
        {
            resultSocket?.Content?.Close();
        }
        return result;
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadCommand(System.String,System.Int32)" />
    public async Task<OperateResult<string[]>> ReadCommandAsync(string command, int lines)
    {
        var buffer = SoftBasic.SpliceArray(Encoding.ASCII.GetBytes(command), new byte[2] { 13, 10 });
        return await ReadFromServerAsync(buffer, lines);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.Reset" />
    public async Task<OperateResult> ResetAsync()
    {
        var read = await ReadCommandAsync("@ RESET ", 1);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.Run" />
    public async Task<OperateResult> RunAsync()
    {
        var read = await ReadCommandAsync("@ RUN ", 1);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.Stop" />
    public async Task<OperateResult> StopAsync()
    {
        var read = await ReadCommandAsync("@ STOP ", 1);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadMotorStatus" />
    public async Task<OperateResult<int>> ReadMotorStatusAsync()
    {
        var read = await ReadCommandAsync("@?MOTOR ", 2);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(read);
        }
        var check = CheckResponseOk(read.Content[1]);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(check);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(read.Content[0]));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadModeStatus" />
    public async Task<OperateResult<int>> ReadModeStatusAsync()
    {
        var read = await ReadCommandAsync("@?MODE ", 2);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(read);
        }
        var check = CheckResponseOk(read.Content[1]);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(check);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(read.Content[0]));
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadJoints" />
    public async Task<OperateResult<float[]>> ReadJointsAsync()
    {
        var read = await ReadCommandAsync("@?WHERE ", 1);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<float[]>(read);
        }
        return OperateResult.CreateSuccessResult((from m in read.Content[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                  select Convert.ToSingle(m)).ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Robot.YAMAHA.YamahaRCX.ReadEmergencyStatus" />
    public async Task<OperateResult<int>> ReadEmergencyStatusAsync()
    {
        var read = await ReadCommandAsync("@?EMG ", 2);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(read);
        }
        var check = CheckResponseOk(read.Content[1]);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(check);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(read.Content[0]));
    }

    /// <summary>
    /// 读取指定的命令的方法，需要指定命令，和接收命令的行数信息<br />
    /// The method of reading the specified command requires the specified command and the line number information of the received command
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="lines">接收的行数信息</param>
    /// <returns>接收的命令</returns>
    [HslMqttApi(Description = "The method of reading the specified command requires the specified command and the line number information of the received command")]
    public OperateResult<string[]> ReadCommand(string command, int lines)
    {
        var send = SoftBasic.SpliceArray(Encoding.ASCII.GetBytes(command), new byte[2] { 13, 10 });
        return ReadFromServer(send, lines);
    }

    private OperateResult CheckResponseOk(string msg)
    {
        if (msg.StartsWith("OK"))
        {
            return OperateResult.CreateSuccessResult();
        }
        return new OperateResult(msg);
    }

    /// <summary>
    /// 指定程序复位信息，对所有的程序进行复位。当重新启动了程序时，从主程序或者任务 1 中最后执行的程序开头开始执行。<br />
    /// Specify the program reset information to reset all programs. When the program is restarted, 
    /// execution starts from the beginning of the main program or the last executed program in task 1.
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    [HslMqttApi(Description = "Specify the program reset information to reset all programs. When the program is restarted")]
    public OperateResult Reset()
    {
        var operateResult = ReadCommand("@ RESET ", 1);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return CheckResponseOk(operateResult.Content[0]);
    }

    /// <summary>
    /// 执行程序运行。执行所有的 RUN 状态程序。<br />
    /// Execute the program to run. Execute all RUN state programs.
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    [HslMqttApi(Description = "Execute the program to run. Execute all RUN state programs.")]
    public OperateResult Run()
    {
        var operateResult = ReadCommand("@ RUN ", 1);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return CheckResponseOk(operateResult.Content[0]);
    }

    /// <summary>
    /// 按照优先顺序 p 将指定的程序登录到任务 n 中。已登录程序变为 STOP 状态。<br />
    /// Logs the specified program into task n in order of p. The logged-in program changes to the STOP state.
    /// </summary>
    /// <param name="program">程序名称</param>
    /// <param name="taskId">任务编号</param>
    /// <returns>是否加载成功</returns>
    public OperateResult Load(string program, int taskId)
    {
        var operateResult = ReadCommand($"＠ LOAD <{program}>, T{taskId}", 1);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return CheckResponseOk(operateResult.Content[0]);
    }

    /// <summary>
    /// 执行程序停止。执行所有的 STOP 状态程序。<br />
    /// The execution program stops. Execute all STOP state programs.
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    [HslMqttApi(Description = "The execution program stops. Execute all STOP state programs.")]
    public OperateResult Stop()
    {
        var operateResult = ReadCommand("@ STOP ", 1);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        return CheckResponseOk(operateResult.Content[0]);
    }

    /// <summary>
    /// 对＜机器人编号＞指定机器人的指定轴进行手动移动（点动）。＜机器人编号＞可以省略。当进行省略时，机器人 1 被指定。
    /// </summary>
    /// <param name="robot">机器人编号</param>
    /// <param name="axis">轴ID信息</param>
    /// <returns>是否操作成功</returns>
    public OperateResult JogXY(int axis, int robot = 1)
    {
        var stringBuilder = new StringBuilder("@ JOGXY ");
        if (robot != 1)
        {
            stringBuilder.Append($"[{robot}] ");
        }
        stringBuilder.Append(Math.Abs(axis).ToString());
        stringBuilder.Append(axis > 0 ? "+" : "-");
        var operateResult = ReadCommand(stringBuilder.ToString(), 2);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        return operateResult;
    }

    /// <summary>
    /// 获取马达电源状态，返回的0:马达电源关闭; 1:马达电源开启; 2:马达电源开启＋所有机器人伺服开启<br />
    /// Get the motor power status, return 0: motor power off; 1: motor power on; 2: motor power on + all robot servos on
    /// </summary>
    /// <returns>返回的0:马达电源关闭; 1:马达电源开启; 2:马达电源开启＋所有机器人伺服开启</returns>
    [HslMqttApi(Description = "Get the motor power status, return 0: motor power off; 1: motor power on; 2: motor power on + all robot servos on")]
    public OperateResult<int> ReadMotorStatus()
    {
        var operateResult = ReadCommand("@?MOTOR ", 2);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var operateResult2 = CheckResponseOk(operateResult.Content[1]);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(operateResult.Content[0]));
    }

    /// <summary>
    /// 读取模式状态<br />
    /// Read mode status
    /// </summary>
    /// <returns>模式的状态信息</returns>
    [HslMqttApi(Description = "Read mode status")]
    public OperateResult<int> ReadModeStatus()
    {
        var operateResult = ReadCommand("@?MODE ", 2);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var operateResult2 = CheckResponseOk(operateResult.Content[1]);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(operateResult.Content[0]));
    }

    /// <summary>
    /// 读取关节的基本数据信息<br />
    /// Read the basic data information of the joint
    /// </summary>
    /// <returns>关节信息</returns>
    [HslMqttApi(Description = "Read the basic data information of the joint")]
    public OperateResult<float[]> ReadJoints()
    {
        var operateResult = ReadCommand("@?WHERE ", 1);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<float[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult((from m in operateResult.Content[0].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                  select Convert.ToSingle(m)).ToArray());
    }

    /// <summary>
    /// 读取紧急停止状态，0 ：正常状态、1 ：紧急停止状态<br />
    /// Read emergency stop state, 0: normal state, 1: emergency stop state
    /// </summary>
    /// <returns>0 ：正常状态、1 ：紧急停止状态</returns>
    [HslMqttApi(Description = "Read emergency stop state, 0: normal state, 1: emergency stop state")]
    public OperateResult<int> ReadEmergencyStatus()
    {
        var operateResult = ReadCommand("@?EMG ", 2);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var operateResult2 = CheckResponseOk(operateResult.Content[1]);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(Convert.ToInt32(operateResult.Content[0]));
    }
}
