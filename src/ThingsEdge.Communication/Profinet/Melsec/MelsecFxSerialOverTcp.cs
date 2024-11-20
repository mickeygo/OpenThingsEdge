using System.Net;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱串口协议的网络版，如果使用的是 FX3U编程口(fx2n) -&gt; GOT1000(RS232)(或是GOT2000)  -&gt; 上位机(以太网) 的方式，那么就需要把<see cref="UseGOT" />设置为 <c>True</c>。
/// </summary>
/// <remarks>
/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="IsNewVersion" />设置为<c>False</c>，如果是FX3U新的型号，则需要设置为<c>True</c>
/// </remarks>
public class MelsecFxSerialOverTcp : DeviceTcpNet, IMelsecFxSerial, IReadWriteNet
{
    private readonly List<string> _inis =
    [
        "00 10 02 FF FF FC 01 10 03",
        "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010201000000023030453032303203364310033543",
        "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010202000000023030454341303203384510033833",
        "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010203000000023030453032303203364310033545",
        "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010204000000023030454341303203384510033835",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102050000000245303138303030343003443510034342",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102060000000245303138303430314303453910034535",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102070000000245303030453030343003453110034436",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102080000000245303030453430343003453510034446",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102090000000245303030453830343003453910034538",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D701020A0000000245303030454330343003463410034630",
    ];

    private readonly SoftIncrementCount _incrementCount = new(2147483647L, 1L);

    /// <summary>
    /// 获取或设置是否使用GOT连接三菱的PLC，当使用了GOT连接到。
    /// </summary>
    public bool UseGOT { get; set; }

    /// <summary>
    /// 当前的编程口协议是否为新版，默认为新版，如果无法读取，切换旧版再次尝试。
    /// </summary>
    public bool IsNewVersion { get; set; }

    /// <summary>
    /// 指定ip地址及端口号来实例化三菱的串口协议的通讯对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public MelsecFxSerialOverTcp(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform()
        {
            IsStringReverseByteWord = true,
        };
        IsNewVersion = true;
        SleepTime = 20;
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        if (UseGOT)
        {
            var array = new byte[66 + command.Length];
            array[0] = 16;
            array[1] = 2;
            array[2] = 94;
            array[6] = 252;
            array[12] = 18;
            array[13] = 18;
            array[17] = byte.MaxValue;
            array[18] = byte.MaxValue;
            array[19] = 3;
            array[22] = byte.MaxValue;
            array[23] = 3;
            array[26] = BitConverter.GetBytes(34 + command.Length)[0];
            array[27] = BitConverter.GetBytes(34 + command.Length)[1];
            array[28] = 28;
            array[29] = 9;
            array[30] = 26;
            array[31] = 24;
            array[41] = 252;
            array[44] = 18;
            array[45] = 18;
            array[46] = 4;
            array[47] = 20;
            array[49] = 1;
            array[50] = BitConverter.GetBytes(Port)[1];
            array[51] = BitConverter.GetBytes(Port)[0];
            array[52] = IPAddress.Parse(IpAddress).GetAddressBytes()[0];
            array[53] = IPAddress.Parse(IpAddress).GetAddressBytes()[1];
            array[54] = IPAddress.Parse(IpAddress).GetAddressBytes()[2];
            array[55] = IPAddress.Parse(IpAddress).GetAddressBytes()[3];
            array[56] = 1;
            array[57] = 2;
            BitConverter.GetBytes((int)_incrementCount.GetCurrentValue()).CopyTo(array, 58);
            command.CopyTo(array, 62);
            array[array.Length - 4] = 16;
            array[array.Length - 3] = 3;
            MelsecHelper.FxCalculateCRC(array, 2, 4).CopyTo(array, array.Length - 2);
            return GetBytesSend(array);
        }
        return base.PackCommandWithHeader(command);
    }

    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        if (UseGOT)
        {
            if (response.Length > 68)
            {
                response = GetBytesReceive(response);
                var num = -1;
                for (var i = 0; i < response.Length - 4; i++)
                {
                    if (response[i] == 16 && response[i + 1] == 2)
                    {
                        num = i;
                        break;
                    }
                }
                if (num >= 0)
                {
                    return OperateResult.CreateSuccessResult(response.RemoveDouble(64 + num, 4));
                }
            }
            return new OperateResult<byte[]>("Got failed: " + response.ToHexString(' ', 16));
        }
        return base.UnpackResponseContent(send, response);
    }

    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(PipeNetBase pipe, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
    {
        var read = await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (read.Content == null)
        {
            return read;
        }
        if (read.Content.Length > 2)
        {
            return read;
        }
        var read2 = await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
        if (!read2.IsSuccess)
        {
            return read2;
        }
        return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(read.Content, read2.Content));
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        if (UseGOT)
        {
            for (var i = 0; i < _inis.Count; i++)
            {
                OperateResult ini1 = await ReadFromCoreServerAsync(Pipe, _inis[i].ToHexBytes(), true, false).ConfigureAwait(false);
                if (!ini1.IsSuccess)
                {
                    return ini1;
                }
            }
        }
        return await base.InitializationOnConnectAsync().ConfigureAwait(false);
    }

    public async Task<OperateResult> ActivePlcAsync()
    {
        return await MelsecFxSerialHelper.ActivePlcAsync(this).ConfigureAwait(false);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadAsync(this, address, length, IsNewVersion).ConfigureAwait(false);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadBoolAsync(this, address, length, IsNewVersion).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, values, IsNewVersion).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, value).ConfigureAwait(false);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] MelsecFxSerialOverTcp -> WriteAsync
        throw new NotImplementedException();
    }

    private static byte[] GetBytesSend(byte[] command)
    {
        var list = new List<byte>();
        for (var i = 0; i < command.Length; i++)
        {
            if (i < 2)
            {
                list.Add(command[i]);
            }
            else if (i < command.Length - 4)
            {
                if (command[i] == 16)
                {
                    list.Add(command[i]);
                }
                list.Add(command[i]);
            }
            else
            {
                list.Add(command[i]);
            }
        }
        return [.. list];
    }

    private static byte[] GetBytesReceive(byte[] response)
    {
        var list = new List<byte>();
        for (var i = 0; i < response.Length; i++)
        {
            if (i < 2)
            {
                list.Add(response[i]);
            }
            else if (i < response.Length - 4)
            {
                if (response[i] == 16 && response[i + 1] == 16)
                {
                    list.Add(response[i]);
                    i++;
                }
                else
                {
                    list.Add(response[i]);
                }
            }
            else
            {
                list.Add(response[i]);
            }
        }
        return [.. list];
    }

    public override string ToString()
    {
        return $"MelsecFxSerialOverTcp[{IpAddress}:{Port}]";
    }
}
