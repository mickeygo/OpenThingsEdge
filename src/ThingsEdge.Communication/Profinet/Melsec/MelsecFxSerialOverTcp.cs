using System.Net;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Profinet.Melsec.Helper;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱串口协议的网络版，如果使用的是 FX3U编程口(fx2n) -&gt; GOT1000(RS232)(或是GOT2000)  -&gt; 上位机(以太网) 的方式，那么就需要把<see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.UseGOT" />设置为 <c>True</c><br />
/// The network version of the Mitsubishi serial port protocol, if you use the FX3U programming port (fx2n) -&gt; GOT1000 (RS232) (or GOT2000) -&gt; host computer (Ethernet) method, 
/// then you need to put <see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.UseGOT" /> is set to <c>True</c>
/// </summary>
/// <remarks>
/// 一般老旧的型号，例如FX2N之类的，需要将<see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.IsNewVersion" />设置为<c>False</c>，如果是FX3U新的型号，则需要将<see cref="P:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.IsNewVersion" />设置为<c>True</c>
/// </remarks>
/// <example>
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
/// </example>
public class MelsecFxSerialOverTcp : DeviceTcpNet, IMelsecFxSerial, IReadWriteNet
{
    private List<string> inis = new List<string>
    {
        "00 10 02 FF FF FC 01 10 03", "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010201000000023030453032303203364310033543", "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010202000000023030454341303203384510033833", "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010203000000023030453032303203364310033545", "10025E000000FC00000000001212000000FFFF030000FF0300002D001C091A18000000000000000000FC000012120414000113960AC4E5D7010204000000023030454341303203384510033835", "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102050000000245303138303030343003443510034342", "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102060000000245303138303430314303453910034535", "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102070000000245303030453030343003453110034436", "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102080000000245303030453430343003453510034446", "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D70102090000000245303030453830343003453910034538",
        "10025E000000FC00000000001212000000FFFF030000FF0300002F001C091A18000000000000000000FC000012120414000113960AC4E5D701020A0000000245303030454330343003463410034630"
    };

    private bool useGot = false;

    private SoftIncrementCount incrementCount;

    /// <summary>
    /// 获取或设置是否使用GOT连接三菱的PLC，当使用了GOT连接到
    /// </summary>
    public bool UseGOT
    {
        get
        {
            return useGot;
        }
        set
        {
            useGot = value;
        }
    }

    /// <summary>
    /// 当前的编程口协议是否为新版，默认为新版，如果无法读取，切换旧版再次尝试<br />
    /// Whether the current programming port protocol is the new version, the default is the new version, 
    /// if it cannot be read, switch to the old version and try again
    /// </summary>
    public bool IsNewVersion { get; set; }

    /// <summary>
    /// 实例化网络版的三菱的串口协议的通讯对象<br />
    /// Instantiate the communication object of Mitsubishi's serial protocol on the network
    /// </summary>
    public MelsecFxSerialOverTcp()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
        IsNewVersion = true;
        ByteTransform.IsStringReverseByteWord = true;
        SleepTime = 20;
        incrementCount = new SoftIncrementCount(2147483647L, 1L);
    }

    /// <summary>
    /// 指定ip地址及端口号来实例化三菱的串口协议的通讯对象<br />
    /// Specify the IP address and port number to instantiate the communication object of Mitsubishi's serial protocol
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    public MelsecFxSerialOverTcp(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    private byte[] GetBytesSend(byte[] command)
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
        return list.ToArray();
    }

    private byte[] GetBytesReceive(byte[] response)
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
        return list.ToArray();
    }

    /// <inheritdoc />
    public override byte[] PackCommandWithHeader(byte[] command)
    {
        if (useGot)
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
            BitConverter.GetBytes((int)incrementCount.GetCurrentValue()).CopyTo(array, 58);
            command.CopyTo(array, 62);
            array[array.Length - 4] = 16;
            array[array.Length - 3] = 3;
            MelsecHelper.FxCalculateCRC(array, 2, 4).CopyTo(array, array.Length - 2);
            return GetBytesSend(array);
        }
        return base.PackCommandWithHeader(command);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> UnpackResponseContent(byte[] send, byte[] response)
    {
        if (useGot)
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

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        if (useGot)
        {
            for (var i = 0; i < inis.Count; i++)
            {
                OperateResult operateResult = ReadFromCoreServer(CommunicationPipe, inis[i].ToHexBytes(), hasResponseData: true, usePackAndUnpack: false);
                if (!operateResult.IsSuccess)
                {
                    return operateResult;
                }
            }
        }
        return base.InitializationOnConnect();
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> ReadFromCoreServer(CommunicationPipe pipe, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
    {
        var operateResult = base.ReadFromCoreServer(pipe, send, hasResponseData, usePackAndUnpack);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        if (operateResult.Content == null)
        {
            return operateResult;
        }
        if (operateResult.Content.Length > 2)
        {
            return operateResult;
        }
        var operateResult2 = base.ReadFromCoreServer(pipe, send, hasResponseData, usePackAndUnpack);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(operateResult.Content, operateResult2.Content));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(CommunicationPipe pipe, byte[] send, bool hasResponseData = true, bool usePackAndUnpack = true)
    {
        var read = await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(continueOnCapturedContext: false);
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
        var read2 = await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(continueOnCapturedContext: false);
        if (!read2.IsSuccess)
        {
            return read2;
        }
        return OperateResult.CreateSuccessResult(SoftBasic.SpliceArray(read.Content, read2.Content));
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        if (useGot)
        {
            for (var i = 0; i < inis.Count; i++)
            {
                OperateResult ini1 = await ReadFromCoreServerAsync(CommunicationPipe, inis[i].ToHexBytes(), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(continueOnCapturedContext: false);
                if (!ini1.IsSuccess)
                {
                    return ini1;
                }
            }
        }
        return await base.InitializationOnConnectAsync();
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.Read(HslCommunication.Core.IReadWriteDevice,System.String,System.UInt16,System.Boolean)" />
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        return MelsecFxSerialHelper.Read(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.Write(HslCommunication.Core.IReadWriteDevice,System.String,System.Byte[],System.Boolean)" />
    public override OperateResult Write(string address, byte[] value)
    {
        return MelsecFxSerialHelper.Write(this, address, value, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Read(System.String,System.UInt16)" />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadAsync(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, value, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ReadBool(HslCommunication.Core.IReadWriteDevice,System.String,System.UInt16,System.Boolean)" />
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        return MelsecFxSerialHelper.ReadBool(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.Write(HslCommunication.Core.IReadWriteDevice,System.String,System.Boolean)" />
    public override OperateResult Write(string address, bool value)
    {
        return MelsecFxSerialHelper.Write(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return await MelsecFxSerialHelper.ReadBoolAsync(this, address, length, IsNewVersion);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.MelsecFxSerialOverTcp.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await MelsecFxSerialHelper.WriteAsync(this, address, value);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    public OperateResult ActivePlc()
    {
        return MelsecFxSerialHelper.ActivePlc(this);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.MelsecFxSerialHelper.ActivePlc(HslCommunication.Core.IReadWriteDevice)" />
    public async Task<OperateResult> ActivePlcAsync()
    {
        return await MelsecFxSerialHelper.ActivePlcAsync(this);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"MelsecFxSerialOverTcp[{IpAddress}:{Port}]";
    }
}
