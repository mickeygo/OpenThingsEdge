using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Core.Net;

/// <summary>
/// 基于连接的CIP协议的基类。
/// </summary>
public abstract class NetworkConnectedCip : DeviceTcpNet
{
    private readonly IncrementCounter _counter = new(65535L, 3L, 2);

    private long _openForwardId = 256L;

    private long _context;

    public uint SessionHandle { get; protected set; }

    /// <summary>
    /// O -&gt; T Network Connection ID
    /// </summary>
    public uint OTConnectionId { get; set; }

    /// <summary>
    /// T -&gt; O Network Connection ID
    /// </summary>
    public uint TOConnectionId { get; set; }

    protected NetworkConnectedCip(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new AllenBradleyMessage();
    }

    /// <inheritdoc />
    protected override byte[] PackCommandWithHeader(byte[] command)
    {
        return AllenBradleyHelper.PackRequestHeader(112, SessionHandle, AllenBradleyHelper.PackCommandSpecificData(GetOTConnectionIdService(), command));
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read1 = await ReadFromCoreServerAsync(NetworkPipe,
            AllenBradleyHelper.RegisterSessionHandle(BitConverter.GetBytes(Interlocked.Increment(ref _context))), true, false)
            .ConfigureAwait(continueOnCapturedContext: false);
        if (!read1.IsSuccess)
        {
            return read1;
        }

        var check = AllenBradleyHelper.CheckResponse(read1.Content);
        if (!check.IsSuccess)
        {
            return check;
        }

        var sessionHandle = ByteTransform.TransUInt32(read1.Content, 4);
        for (var i = 0; i < 10; i++)
        {
            var id = Interlocked.Increment(ref _openForwardId);
            var send = AllenBradleyHelper.PackRequestHeader(
                111,
                sessionHandle,
                GetLargeForwardOpen(i < 7 ? (ushort)i : (ushort)RandomExtensions.Random.Next(7, 200)),
                ByteTransform.TransByte(id));
            var read2 = await ReadFromCoreServerAsync(NetworkPipe, send, true, false).ConfigureAwait(false);
            if (!read2.IsSuccess)
            {
                return read2;
            }

            try
            {
                if (read2.Content.Length >= 46 && read2.Content[42] != 0)
                {
                    var err = ByteTransform.TransUInt16(read2.Content, 44);
                    if (err == 256 && i < 9)
                    {
                        continue;
                    }
                    return err switch
                    {
                        256 => new OperateResult("Connection in use or duplicate Forward Open"),
                        275 => new OperateResult("Extended Status: Out of connections (0x0113)"),
                        _ => new OperateResult("Forward Open failed, Code: " + ByteTransform.TransUInt16(read2.Content, 44)),
                    };
                }
                OTConnectionId = ByteTransform.TransUInt32(read2.Content, 44);
            }
            catch (Exception ex2)
            {
                var ex = ex2;
                return new OperateResult(ex.Message + Environment.NewLine + "Source: " + read2.Content.ToHexString(' '));
            }
            break;
        }
        _counter.Reset();
        SessionHandle = sessionHandle;
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> ExtraOnDisconnectAsync()
    {
        var forwardClose = GetLargeForwardClose();
        if (forwardClose != null)
        {
            var close = await ReadFromCoreServerAsync(NetworkPipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, forwardClose), true, false).ConfigureAwait(false);
            if (!close.IsSuccess)
            {
                return close;
            }
        }
        var read = await ReadFromCoreServerAsync(NetworkPipe, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle), true, false).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 将多个的CIP命令打包成一个服务的命令
    /// </summary>
    /// <param name="cip">CIP命令列表</param>
    /// <returns>服务命令</returns>
    protected byte[] PackCommandService(params byte[][] cip)
    {
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(177);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        var currentValue = _counter.OnNext();
        memoryStream.WriteByte(BitConverter.GetBytes(currentValue)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(currentValue)[1]);
        if (cip.Length == 1)
        {
            memoryStream.Write(cip[0], 0, cip[0].Length);
        }
        else
        {
            memoryStream.Write([10, 2, 32, 2, 36, 1], 0, 6);
            memoryStream.WriteByte(BitConverter.GetBytes(cip.Length)[0]);
            memoryStream.WriteByte(BitConverter.GetBytes(cip.Length)[1]);
            var num = 2 + cip.Length * 2;
            for (var i = 0; i < cip.Length; i++)
            {
                memoryStream.WriteByte(BitConverter.GetBytes(num)[0]);
                memoryStream.WriteByte(BitConverter.GetBytes(num)[1]);
                num += cip[i].Length;
            }
            for (var j = 0; j < cip.Length; j++)
            {
                memoryStream.Write(cip[j], 0, cip[j].Length);
            }
        }
        var array = memoryStream.ToArray();
        memoryStream.Dispose();
        BitConverter.GetBytes((ushort)(array.Length - 4)).CopyTo(array, 2);
        return array;
    }

    /// <summary>
    /// 获取数据通信的前置打开命令，不同的PLC的信息不一样。
    /// </summary>
    /// <param name="connectionID">连接的ID信息</param>
    /// <returns>原始命令数据</returns>
    protected virtual byte[] GetLargeForwardOpen(ushort connectionID)
    {
        return "\r\n00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00\r\n5b 02 20 06 24 01 0e 9c 02 00 00 80 01 00 fe 80\r\n02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00\r\ncc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02\r\n24 01 2c 01".ToHexBytes();
    }

    /// <summary>
    /// 获取数据通信的后置关闭命令，不同的PLC的信息不一样。
    /// </summary>
    /// <returns>原始命令数据</returns>
    protected virtual byte[]? GetLargeForwardClose()
    {
        return null;
    }

    private byte[] GetOTConnectionIdService()
    {
        var array = new byte[8] { 161, 0, 4, 0, 0, 0, 0, 0 };
        ByteTransform.TransByte(OTConnectionId).CopyTo(array, 4);
        return array;
    }

    /// <summary>
    /// 从PLC反馈的数据解析出真实的数据内容，结果内容分别是原始字节数据，数据类型代码，是否有很多的数据。
    /// </summary>
    /// <param name="response">PLC的反馈数据</param>
    /// <param name="isRead">是否是返回的操作</param>
    /// <returns>带有结果标识的最终数据</returns>
    public static OperateResult<byte[], ushort, bool> ExtractActualData(byte[] response, bool isRead)
    {
        var list = new List<byte>();
        try
        {
            var num = 42;
            var value = false;
            ushort value2 = 0;
            var num2 = BitConverter.ToUInt16(response, num);
            if (BitConverter.ToInt32(response, 46) == 138)
            {
                num = 50;
                int num3 = BitConverter.ToUInt16(response, num);
                for (var i = 0; i < num3; i++)
                {
                    var num4 = BitConverter.ToUInt16(response, num + 2 + i * 2) + num;
                    var num5 = i == num3 - 1 ? response.Length : BitConverter.ToUInt16(response, num + 4 + i * 2) + num;
                    var num6 = BitConverter.ToUInt16(response, num4 + 2);
                    switch (num6)
                    {
                        case 4:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley04
                            };
                        case 5:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley05
                            };
                        case 6:
                            if (response[num + 2] == 210 || response[num + 2] == 204)
                            {
                                return new OperateResult<byte[], ushort, bool>
                                {
                                    ErrorCode = num6,
                                    Message = StringResources.Language.AllenBradley06
                                };
                            }
                            break;
                        case 10:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley0A
                            };
                        case 12:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley0C
                            };
                        case 19:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley13
                            };
                        case 28:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley1C
                            };
                        case 30:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley1E
                            };
                        case 38:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.AllenBradley26
                            };
                        default:
                            return new OperateResult<byte[], ushort, bool>
                            {
                                ErrorCode = num6,
                                Message = StringResources.Language.UnknownError
                            };
                        case 0:
                            break;
                    }
                    if (isRead)
                    {
                        for (var j = num4 + 6; j < num5; j++)
                        {
                            list.Add(response[j]);
                        }
                    }
                }
            }
            else
            {
                var b = response[num + 6];
                switch (b)
                {
                    case 4:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley04
                        };
                    case 5:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley05
                        };
                    case 6:
                        value = true;
                        break;
                    case 10:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley0A
                        };
                    case 12:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley0C
                        };
                    case 19:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley13
                        };
                    case 28:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley1C
                        };
                    case 30:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley1E
                        };
                    case 38:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.AllenBradley26
                        };
                    default:
                        return new OperateResult<byte[], ushort, bool>
                        {
                            ErrorCode = b,
                            Message = StringResources.Language.UnknownError
                        };
                    case 0:
                        break;
                }
                if (response[num + 4] == 205 || response[num + 4] == 211)
                {
                    return OperateResult.CreateSuccessResult(list.ToArray(), value2, value);
                }
                if (response[num + 4] == 204 || response[num + 4] == 210)
                {
                    for (var k = num + 10; k < num + 2 + num2; k++)
                    {
                        list.Add(response[k]);
                    }
                    value2 = BitConverter.ToUInt16(response, num + 8);
                }
                else if (response[num + 4] == 213)
                {
                    for (var l = num + 8; l < num + 2 + num2; l++)
                    {
                        list.Add(response[l]);
                    }
                }
                else if (response[num + 4] == 203)
                {
                    if (response[58] != 0)
                    {
                        return new OperateResult<byte[], ushort, bool>(response[58], AllenBradleyDF1Serial.GetExtStatusDescription(response[58]) + Environment.NewLine + "Source: " + response.RemoveBegin(57).ToHexString(' '));
                    }
                    if (!isRead)
                    {
                        return OperateResult.CreateSuccessResult(list.ToArray(), value2, value);
                    }
                    return OperateResult.CreateSuccessResult(response.RemoveBegin(61), value2, value);
                }
            }
            return OperateResult.CreateSuccessResult(list.ToArray(), value2, value);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[], ushort, bool>("ExtractActualData failed: " + ex.Message + Environment.NewLine + "Source: " + response.ToHexString(' '));
        }
    }
}
