using System.Net.Sockets;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Net;

namespace ThingsEdge.Communication.Robot.YAMAHA;

/// <summary>
/// 雅马哈机器人的数据访问类
/// </summary>
public sealed class YamahaRCX : NetworkDoubleBase
{
    /// <summary>
    /// 指定IP地址和端口来实例化一个对象
    /// </summary>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="port">端口号</param>
    public YamahaRCX(string ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
        ByteTransform = new RegularByteTransform();
        ReceiveTimeOut = 30_000;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new SpecifiedCharacterMessage(13, 10);
    }

    public async Task<OperateResult<string[]>> ReadFromServerAsync(byte[] send, int lines)
    {
        var result = new OperateResult<string[]>();
        pipeSocket.PipeLockEnter();

        OperateResult<Socket> resultSocket;
        try
        {
            resultSocket = await GetAvailableSocketAsync().ConfigureAwait(false);
            if (!resultSocket.IsSuccess)
            {
                pipeSocket.IsSocketError = true;
                AlienSession?.Offline();
                result.CopyErrorFromOther(resultSocket);
                return result;
            }

            var buffers = new List<string>();
            var isError = false;
            for (var i = 0; i < lines; i++)
            {
                var read = await ReadFromCoreServerAsync(resultSocket.Content, send).ConfigureAwait(false);
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
                result.Content = [.. buffers];
                result.Message = StringResources.Language.SuccessText;
            }
            ExtraAfterReadFromCoreServer(new OperateResult
            {
                IsSuccess = !isError
            });
        }
        catch
        {
            throw;
        }
        finally
        {
            pipeSocket.PipeLockLeave();
        }

        if (!IsPersistentConn)
        {
            resultSocket?.Content?.Close();
        }
        return result;
    }

    /// <summary>
    /// 读取指定的命令的方法，需要指定命令，和接收命令的行数信息。
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="lines">接收的行数信息</param>
    /// <returns>接收的命令</returns>
    public async Task<OperateResult<string[]>> ReadCommandAsync(string command, int lines)
    {
        var buffer = SoftBasic.SpliceArray(Encoding.ASCII.GetBytes(command), "\r\n"u8.ToArray());
        return await ReadFromServerAsync(buffer, lines).ConfigureAwait(false);
    }

    /// <summary>
    /// 指定程序复位信息，对所有的程序进行复位。当重新启动了程序时，从主程序或者任务 1 中最后执行的程序开头开始执行。
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    public async Task<OperateResult> ResetAsync()
    {
        var read = await ReadCommandAsync("@ RESET ", 1).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <summary>
    /// 执行程序运行。执行所有的 RUN 状态程序。
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    public async Task<OperateResult> RunAsync()
    {
        var read = await ReadCommandAsync("@ RUN ", 1).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <summary>
    /// 执行程序停止。执行所有的 STOP 状态程序。
    /// </summary>
    /// <returns>执行结果是否成功</returns>
    public async Task<OperateResult> StopAsync()
    {
        var read = await ReadCommandAsync("@ STOP ", 1).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseOk(read.Content[0]);
    }

    /// <summary>
    /// 获取马达电源状态，返回的0:马达电源关闭; 1:马达电源开启; 2:马达电源开启＋所有机器人伺服开启。
    /// </summary>
    /// <returns>返回的0:马达电源关闭; 1:马达电源开启; 2:马达电源开启＋所有机器人伺服开启</returns>
    public async Task<OperateResult<int>> ReadMotorStatusAsync()
    {
        var read = await ReadCommandAsync("@?MOTOR ", 2).ConfigureAwait(false);
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
    /// 读取模式状态。
    /// </summary>
    /// <returns>模式的状态信息</returns>
    public async Task<OperateResult<int>> ReadModeStatusAsync()
    {
        var read = await ReadCommandAsync("@?MODE ", 2).ConfigureAwait(false);
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
    /// 读取关节的基本数据信息.
    /// </summary>
    /// <returns>关节信息</returns>
    public async Task<OperateResult<float[]>> ReadJointsAsync()
    {
        var read = await ReadCommandAsync("@?WHERE ", 1).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<float[]>(read);
        }
        return OperateResult.CreateSuccessResult((from m in read.Content[0].Split([' '], StringSplitOptions.RemoveEmptyEntries)
                                                  select Convert.ToSingle(m)).ToArray());
    }

    /// <summary>
    /// 读取紧急停止状态，0 ：正常状态、1 ：紧急停止状态。
    /// </summary>
    /// <returns>0 ：正常状态、1 ：紧急停止状态</returns>
    public async Task<OperateResult<int>> ReadEmergencyStatusAsync()
    {
        var read = await ReadCommandAsync("@?EMG ", 2).ConfigureAwait(false);
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

    private static OperateResult CheckResponseOk(string msg)
    {
        if (msg.StartsWith("OK"))
        {
            return OperateResult.CreateSuccessResult();
        }
        return new OperateResult(msg);
    }

    /// <summary>
    /// 按照优先顺序 p 将指定的程序登录到任务 n 中。已登录程序变为 STOP 状态。
    /// </summary>
    /// <param name="program">程序名称</param>
    /// <param name="taskId">任务编号</param>
    /// <returns>是否加载成功</returns>
    public async Task<OperateResult> LoadAsync(string program, int taskId)
    {
        var operateResult = await ReadCommandAsync($"＠ LOAD <{program}>, T{taskId}", 1).ConfigureAwait(false);
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
    public async Task<OperateResult> JogXYAsync(int axis, int robot = 1)
    {
        var stringBuilder = new StringBuilder("@ JOGXY ");
        if (robot != 1)
        {
            stringBuilder.Append($"[{robot}] ");
        }
        stringBuilder.Append(Math.Abs(axis));
        stringBuilder.Append(axis > 0 ? "+" : "-");
        var operateResult = await ReadCommandAsync(stringBuilder.ToString(), 2).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        return operateResult;
    }
}
