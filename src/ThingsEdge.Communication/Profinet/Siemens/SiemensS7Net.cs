using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.Pipe;
using ThingsEdge.Communication.Core.IMessage;
using ThingsEdge.Communication.Core.Address;
using ThingsEdge.Communication.Profinet.Siemens.Helper;
using ThingsEdge.Communication.Exceptions;
using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <summary>
/// 一个西门子的客户端类，使用S7协议来进行数据交互，对于s300,s400需要关注<see cref="Slot" />和<see cref="Rack" />的设置值，
/// 对于s200，需要关注<see cref="LocalTSAP" />和<see cref="DestTSAP" />的设置值，详细参考demo的设置。
/// </summary>
/// <remarks>
/// 暂时不支持bool[]的批量写入操作，请使用 Write(string, byte[]) 替换。
/// 对于200smartPLC的V区，就是DB1.X，例如，V100=DB1.100，当然了你也可以输入V100。
/// 在西门子PLC，字符串分为普通的 String 和 WString 类型，前者为单字节的类型，后者为双字节的字符串类型。
/// 一个字符串除了本身的数据信息，还有字符串的长度信息，比如字符串 "12345"，比如在PLC的地址 DB1.0 存储的字节是 FE 05 31 32 33 34 35, 第一个字节是最大长度，第二个字节是当前长度，后面的才是字符串的数据信息。
/// </remarks>
public sealed class SiemensS7Net : DeviceTcpNet
{
    private readonly SiemensPLCS _currentPlc = SiemensPLCS.S1200;

    /// <summary>
    /// 第一次初始化指令交互报文
    /// </summary>
    private byte[] _plcHead1 =
    [
        3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
        0, 192, 1, 10, 193, 2, 1, 2, 194, 2,
        1, 0
    ];

    /// <summary>
    /// 第二次初始化指令交互报文
    /// </summary>
    private byte[] _plcHead2 =
    [
        3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
        0, 4, 0, 0, 8, 0, 0, 240, 0, 0,
        1, 0, 1, 1, 224
    ];

    private readonly byte[] _plcOrderNumber =
    [
        3, 0, 0, 33, 2, 240, 128, 50, 7, 0,
        0, 0, 1, 0, 8, 0, 8, 0, 1, 18,
        4, 17, 68, 1, 0, 255, 9, 0, 4, 0,
        17, 0, 0
    ];

    private readonly byte[] _plcHead1_200smart =
    [
        3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
        0, 193, 2, 16, 0, 194, 2, 3, 0, 192,
        1, 10
    ];

    /// <summary>
    /// 第二次初始化指令交互报文
    /// </summary>
    private readonly byte[] _plcHead2_200smart =
    [
        3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
        0, 204, 193, 0, 8, 0, 0, 240, 0, 0,
        1, 0, 1, 3, 192
    ];

    private readonly byte[] _plcHead1_200 =
    [
        3, 0, 0, 22, 17, 224, 0, 0, 0, 1,
        0, 193, 2, 77, 87, 194, 2, 77, 87, 192,
        1, 9
    ];

    private readonly byte[] _plcHead2_200 =
    [
        3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
        0, 0, 0, 0, 8, 0, 0, 240, 0, 0,
        1, 0, 1, 3, 192
    ];

    private readonly byte[] _s7_STOP =
    [
        3, 0, 0, 33, 2, 240, 128, 50, 1, 0,
        0, 14, 0, 0, 16, 0, 0, 41, 0, 0,
        0, 0, 0, 9, 80, 95, 80, 82, 79, 71,
        82, 65, 77
    ];

    private readonly byte[] _s7_HOT_START =
    [
        3, 0, 0, 37, 2, 240, 128, 50, 1, 0,
        0, 12, 0, 0, 20, 0, 0, 40, 0, 0,
        0, 0, 0, 0, 253, 0, 0, 9, 80, 95,
        80, 82, 79, 71, 82, 65, 77
    ];

    private readonly byte[] _s7_COLD_START =
    [
        3, 0, 0, 39, 2, 240, 128, 50, 1, 0,
        0, 15, 0, 0, 22, 0, 0, 40, 0, 0,
        0, 0, 0, 0, 253, 0, 2, 67, 32, 9,
        80, 95, 80, 82, 79, 71, 82, 65, 77
    ];

    private byte _plc_rack;
    private byte _plc_slot;

    private IncrementCounter _incrementCount = new(65535L, 1L);

    /// <summary>
    /// PLC的槽号，针对S7-400的PLC设置的。
    /// </summary>
    public byte Slot
    {
        get => _plc_slot;
        set
        {
            _plc_slot = value;
            if (_currentPlc != SiemensPLCS.S200 && _currentPlc != SiemensPLCS.S200Smart)
            {
                _plcHead1[21] = (byte)(_plc_rack * 32 + _plc_slot);
            }
        }
    }

    /// <summary>
    /// PLC的机架号，针对S7-400的PLC设置的。
    /// </summary>
    public byte Rack
    {
        get => _plc_rack;
        set
        {
            _plc_rack = value;
            if (_currentPlc != SiemensPLCS.S200 && _currentPlc != SiemensPLCS.S200Smart)
            {
                _plcHead1[21] = (byte)(_plc_rack * 32 + _plc_slot);
            }
        }
    }

    /// <summary>
    /// 获取或设置当前PLC的连接方式，PG: 0x01，OP: 0x02，S7Basic: 0x03...0x10。
    /// </summary>
    public byte ConnectionType
    {
        get => _plcHead1[20];
        set
        {
            if (_currentPlc is not SiemensPLCS.S200 and not SiemensPLCS.S200Smart)
            {
                _plcHead1[20] = value;
            }
        }
    }

    /// <summary>
    /// 西门子相关的本地TSAP参数信息。
    /// </summary>
    public int LocalTSAP
    {
        get
        {
            if (_currentPlc is SiemensPLCS.S200 or SiemensPLCS.S200Smart)
            {
                return _plcHead1[13] * 256 + _plcHead1[14];
            }
            return _plcHead1[16] * 256 + _plcHead1[17];
        }
        set
        {
            if (_currentPlc is SiemensPLCS.S200 or SiemensPLCS.S200Smart)
            {
                _plcHead1[13] = BitConverter.GetBytes(value)[1];
                _plcHead1[14] = BitConverter.GetBytes(value)[0];
            }
            else
            {
                _plcHead1[16] = BitConverter.GetBytes(value)[1];
                _plcHead1[17] = BitConverter.GetBytes(value)[0];
            }
        }
    }

    /// <summary>
    /// 西门子相关的远程TSAP参数信息。
    /// </summary>
    public int DestTSAP
    {
        get
        {
            if (_currentPlc is SiemensPLCS.S200 or SiemensPLCS.S200Smart)
            {
                return _plcHead1[17] * 256 + _plcHead1[18];
            }
            return _plcHead1[20] * 256 + _plcHead1[21];
        }
        set
        {
            if (_currentPlc is SiemensPLCS.S200 or SiemensPLCS.S200Smart)
            {
                _plcHead1[17] = BitConverter.GetBytes(value)[1];
                _plcHead1[18] = BitConverter.GetBytes(value)[0];
            }
            else
            {
                _plcHead1[20] = BitConverter.GetBytes(value)[1];
                _plcHead1[21] = BitConverter.GetBytes(value)[0];
            }
        }
    }

    /// <summary>
    /// 获取当前西门子的PDU的长度信息，不同型号PLC的值会不一样。
    /// </summary>
    public int PDULength { get; private set; } = 200;

    /// <summary>
    /// 实例化一个西门子的S7协议的通讯对象并指定Ip地址。
    /// </summary>
    /// <param name="siemens">指定西门子的型号</param>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号，默认 102</param>
    public SiemensS7Net(SiemensPLCS siemens, string ipAddress, int port = 102) : base(ipAddress, port)
    {
        WordLength = 2;
        ByteTransform = new ReverseBytesTransform();
        _currentPlc = siemens;

        Initialization(siemens);
    }

    /// <inheritdoc />
    protected override INetMessage GetNewNetMessage()
    {
        return new S7Message();
    }

    private void Initialization(SiemensPLCS siemens)
    {
        switch (siemens)
        {
            case SiemensPLCS.S1200:
                _plcHead1[21] = 0;
                break;
            case SiemensPLCS.S300:
                _plcHead1[21] = 2;
                break;
            case SiemensPLCS.S400:
                _plcHead1[21] = 3;
                _plcHead1[17] = 0;
                break;
            case SiemensPLCS.S1500:
                _plcHead1[21] = 0;
                break;
            case SiemensPLCS.S200Smart:
                _plcHead1 = _plcHead1_200smart;
                _plcHead2 = _plcHead2_200smart;
                break;
            case SiemensPLCS.S200:
                _plcHead1 = _plcHead1_200;
                _plcHead2 = _plcHead2_200;
                break;
            default:
                _plcHead1[18] = 0;
                break;
        }
    }

    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var read_first = await ReadFromCoreServerAsync(NetworkPipe, _plcHead1, hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(false);
        if (!read_first.IsSuccess)
        {
            return read_first;
        }
        var read_second = await ReadFromCoreServerAsync(NetworkPipe, _plcHead2, hasResponseData: true, usePackAndUnpack: true).ConfigureAwait(false);
        if (!read_second.IsSuccess)
        {
            return read_second;
        }
        PDULength = ByteTransform.TransUInt16(read_second.Content.SelectLast(2), 0) - 28;
        if (PDULength < 200)
        {
            PDULength = 200;
        }
        _incrementCount = new IncrementCounter(65535L, 1L);
        return OperateResult.CreateSuccessResult();
    }

    protected override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(NetworkPipeBase pipe, byte[] send, bool hasResponseData, bool usePackAndUnpack)
    {
        OperateResult<byte[]> read;
        byte[] content;
        do
        {
            read = await base.ReadFromCoreServerAsync(pipe, send, hasResponseData, usePackAndUnpack).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            content = read.Content;
        }
        while (content == null || content.Length < 4 || read.Content![2] * 256 + read.Content[3] == 7);
        return read;
    }

    /// <summary>
    /// 从PLC读取订货号信息。
    /// </summary>
    /// <returns>CPU的订货号信息</returns>
    public async Task<OperateResult<string>> ReadOrderNumberAsync()
    {
        var read = await ReadFromCoreServerAsync(_plcOrderNumber).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        if (read.Content == null || read.Content.Length < 91)
        {
            return new OperateResult<string>(StringResources.Language.ReceiveDataLengthTooShort + "91, Source: " + read.Content?.ToHexString(' '));
        }
        return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 71, 20));
    }

    /// <summary>
    /// 热启动
    /// </summary>
    /// <returns></returns>
    public async Task<OperateResult> HotStartAsync()
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(_s7_HOT_START).ConfigureAwait(false), CheckStartResult);
    }

    /// <summary>
    /// 冷启动
    /// </summary>
    /// <returns></returns>
    public async Task<OperateResult> ColdStartAsync()
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(_s7_COLD_START).ConfigureAwait(false), CheckStartResult);
    }

    /// <summary>
    /// 停止
    /// </summary>
    /// <returns></returns>
    public async Task<OperateResult> StopAsync()
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(_s7_STOP).ConfigureAwait(false), CheckStopResult);
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var addressResult = S7AddressData.ParseFrom(address, length);
        if (!addressResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(addressResult);
        }
        return await ReadAsync([addressResult.Content]).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量读取数据。
    /// </summary>
    /// <param name="addresses">地址集合</param>
    /// <param name="lengths">与地址对应的长度集合</param>
    /// <returns></returns>
    public async Task<OperateResult<byte[]>> ReadAsync(string[] addresses, ushort[] lengths)
    {
        var addressResult = new S7AddressData[addresses.Length];
        for (var i = 0; i < addresses.Length; i++)
        {
            var tmp = S7AddressData.ParseFrom(addresses[i], lengths[i]);
            if (!tmp.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(tmp);
            }
            addressResult[i] = tmp.Content;
        }
        return await ReadAsync(addressResult).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量读取数据。
    /// </summary>
    /// <param name="s7Addresses">地址数据集合。</param>
    /// <returns></returns>
    public async Task<OperateResult<byte[]>> ReadAsync(S7AddressData[] s7Addresses)
    {
        var bytes = new List<byte>();
        var groups = SiemensS7Helper.ArraySplitByLength(s7Addresses, PDULength);
        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            if (group.Length == 1 && group[0].Length > PDULength)
            {
                var array = SiemensS7Helper.SplitS7Address(group[0], PDULength);
                for (var j = 0; j < array.Length; j++)
                {
                    var read = await ReadS7AddressDataAsync([array[j]]).ConfigureAwait(false);
                    if (!read.IsSuccess)
                    {
                        return read;
                    }
                    bytes.AddRange(read.Content);
                }
            }
            else
            {
                var read = await ReadS7AddressDataAsync(group).ConfigureAwait(false);
                if (!read.IsSuccess)
                {
                    return read;
                }
                bytes.AddRange(read.Content);
            }
        }
        return OperateResult.CreateSuccessResult(bytes.ToArray());
    }

    private async Task<OperateResult<byte[]>> ReadS7AddressDataAsync(S7AddressData[] s7Addresses)
    {
        var command = BuildReadCommand(s7Addresses, GetMessageId());
        if (!command.IsSuccess)
        {
            return command;
        }

        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return AnalysisReadByte(s7Addresses, read.Content);
    }

    private async Task<OperateResult<byte[]>> ReadBitFromPLCAsync(string address)
    {
        var command = BuildBitReadCommand(address, GetMessageId());
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return SiemensS7Helper.AnalysisReadBit(read.Content);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        var analysis = S7AddressData.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(analysis);
        }
        return await WriteAsync(analysis.Content, data).ConfigureAwait(false);
    }

    private async Task<OperateResult> WriteAsync(S7AddressData address, byte[] values)
    {
        var length = values.Length;
        ushort alreadyFinished = 0;
        while (alreadyFinished < length)
        {
            var writeLength = (ushort)Math.Min(length - alreadyFinished, PDULength);
            var buffer = ByteTransform.TransByte(values, alreadyFinished, writeLength);
            var command = BuildWriteByteCommand(address, buffer, GetMessageId());
            if (!command.IsSuccess)
            {
                return command;
            }
            var write = await WriteBaseAsync(command.Content).ConfigureAwait(false);
            if (!write.IsSuccess)
            {
                return write;
            }
            alreadyFinished += writeLength;
            address.AddressStart += writeLength * 8;
        }
        return OperateResult.CreateSuccessResult();
    }
  
    public override async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadBitFromPLCAsync(address).ConfigureAwait(false), (m) => m[0] != 0);
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        var analysis = S7AddressData.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }

        CommHelper.CalculateStartBitIndexAndLength(analysis.Content.AddressStart, length, out var newStart, out var byteLength, out var offset);
        analysis.Content.AddressStart = newStart;
        analysis.Content.Length = byteLength;
        var read = await ReadAsync([analysis.Content]).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.ToBoolArray().SelectMiddle(offset, length));
    }

    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        return length != 0 ? await base.ReadStringAsync(address, length, encoding).ConfigureAwait(false) : await ReadStringAsync(address, encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步使用指定的编码，读取字符串数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns></returns>
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return await ReadStringAsync(address, Encoding.ASCII).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步使用指定的编码，读取字符串数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="encoding">指定的自定义的编码</param>
    /// <returns>带有成功标识的string数据</returns>
    public async Task<OperateResult<string>> ReadStringAsync(string address, Encoding encoding)
    {
        return await SiemensS7Helper.ReadStringAsync(this, _currentPlc, address, encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步使用指定的编码，读取字符串数据。
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <returns>带有成功标识的string数据</returns>
    public async Task<OperateResult<string>> ReadWStringAsync(string address)
    {
        return await SiemensS7Helper.ReadWStringAsync(this, _currentPlc, address).ConfigureAwait(false);
    }

    public async Task<OperateResult<DateTime>> ReadDateAsync(string address)
    {
        return (await ReadUInt16Async(address).ConfigureAwait(false)).Then((m) => OperateResult.CreateSuccessResult(new DateTime(1990, 1, 1).AddDays(m)));
    }

    public async Task<OperateResult<DateTime>> ReadDateTimeAsync(string address)
    {
        var read = await ReadAsync(address, 8).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(read);
        }
        return SiemensDateTime.FromByteArray(read.Content!);
    }

    public async Task<OperateResult<DateTime>> ReadDTLDataTimeAsync(string address)
    {
        var read = await ReadAsync(address, 12).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<DateTime>(read);
        }
        return SiemensDateTime.GetDTLTime(ByteTransform, read.Content!, 0);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        var command = BuildWriteBitCommand(address, value, GetMessageId());
        if (!command.IsSuccess)
        {
            return command;
        }
        return await WriteBaseAsync(command.Content).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        var analysis = S7AddressData.ParseFrom(address);
        if (!analysis.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(analysis);
        }

        CommHelper.CalculateStartBitIndexAndLength(analysis.Content.AddressStart, (ushort)values.Length, out var newStart, out var byteLength, out var offset);
        analysis.Content.AddressStart = newStart;
        analysis.Content.Length = byteLength;
        var read = await ReadAsync([analysis.Content]).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }

        var boolArray = read.Content!.ToBoolArray();
        Array.Copy(values, 0, boolArray, offset, values.Length);
        return await WriteAsync(analysis.Content, boolArray.ToByteArray()).ConfigureAwait(false);
    }

    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        return await SiemensS7Helper.WriteAsync(this, _currentPlc, address, value, encoding).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteAsync(string[] addresses, List<byte[]> data)
    {
        var addressResult = new S7AddressData[addresses.Length];
        for (var i = 0; i < addresses.Length; i++)
        {
            var tmp = S7AddressData.ParseFrom(addresses[i], 1);
            if (!tmp.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(tmp);
            }
            addressResult[i] = tmp.Content;
        }
        var command = BuildWriteByteCommand(addressResult, data, GetMessageId());
        if (!command.IsSuccess)
        {
            return command;
        }

        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return AnalysisWrite(read.Content);
    }

    public async Task<OperateResult> WriteWStringAsync(string address, string value)
    {
        return await SiemensS7Helper.WriteWStringAsync(this, _currentPlc, address, value).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteAsync(string address, DateTime dateTime)
    {
        return await WriteAsync(address, SiemensDateTime.ToByteArray(dateTime)).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteDTLTimeAsync(string address, DateTime dateTime)
    {
        return await WriteAsync(address, SiemensDateTime.GetBytesFromDTLTime(ByteTransform, dateTime)).ConfigureAwait(false);
    }

    public async Task<OperateResult> WriteDateAsync(string address, DateTime dateTime)
    {
        return await WriteAsync(address, Convert.ToUInt16((dateTime - new DateTime(1990, 1, 1)).TotalDays)).ConfigureAwait(false);
    }

    private async Task<OperateResult> WriteBaseAsync(byte[] entireValue)
    {
        return ByteTransformHelper.GetResultFromOther(await ReadFromCoreServerAsync(entireValue).ConfigureAwait(false), AnalysisWrite);
    }

    /// <summary>
    /// 强制输出一个位到指定的地址，针对PLC类型为 200smart 时有效。
    /// </summary>
    /// <remarks>测试型号: S7-200 smart CPU SR30</remarks>
    /// <param name="address">西门子的地址信息，例如 I0.0, Q1.0, M2.0</param>
    /// <param name="value">输出值 false=断开, true=闭合</param>
    /// <returns>是否强制输出成功</returns>
    public async Task<OperateResult> ForceBoolAsync(string address, bool value)
    {
        var operateResult = S7AddressData.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }

        var array = new byte[47]
        {
            3, 0, 0, 47, 2, 240, 128, 50, 7, 0,
            0, 121, 249, 0, 12, 0, 18, 0, 1, 18,
            8, 18, 72, 11, 0, 0, 0, 0, 0, 255,
            9, 0, 14, 0, 1, 16, 1, 0, 1, 0,
            0, 130, 0, 0, 0, 1, 0
        };
        array[39] = (byte)(operateResult.Content!.DbBlock / 256);
        array[40] = (byte)(operateResult.Content.DbBlock % 256);
        array[41] = operateResult.Content.DataCode;
        array[42] = (byte)(operateResult.Content.AddressStart / 256 / 256 % 256);
        array[43] = (byte)(operateResult.Content.AddressStart / 256 % 256);
        array[44] = (byte)(operateResult.Content.AddressStart % 256);
        array[45] = (byte)(value ? 1u : 0u);

        var operateResult2 = await ReadFromCoreServerAsync(array).ConfigureAwait(false);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }

        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 取消所有强制输出，针对PLC类型为 200smart 时有效。
    /// </summary>
    /// <returns>是否取消成功</returns>
    public async Task<OperateResult> CancelAllForceAsync()
    {
        var send = new byte[35]
        {
            3, 0, 0, 35, 2, 240, 128, 50, 7, 0,
            0, 166, 209, 0, 12, 0, 6, 0, 1, 18,
            8, 18, 72, 11, 0, 0, 0, 0, 0, 255,
            9, 0, 2, 2, 0
        };
        var operateResult = await ReadFromCoreServerAsync(send).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }

        if (operateResult.Content!.Length < 33)
        {
            return new OperateResult("Receive error");
        }

        if (operateResult.Content[11] != 166 || operateResult.Content[12] != 209)
        {
            return new OperateResult("CancelAllForceOut Fail!");
        }

        return OperateResult.CreateSuccessResult();
    }

    private int GetMessageId()
    {
        return (int)_incrementCount.OnNext();
    }

    public override string ToString()
    {
        return $"SiemensS7Net {_currentPlc}[{IpAddress}:{Port}]";
    }

    /// <summary>
    /// A general method for generating a command header to read a Word data
    /// </summary>
    /// <param name="s7Addresses">siemens address</param>
    /// <param name="msgId">message id informaion</param>
    /// <returns>Message containing the result object</returns>
    /// 
    private static OperateResult<byte[]> BuildReadCommand(S7AddressData[] s7Addresses, int msgId)
    {
        if (s7Addresses.Length > 19)
        {
            throw new CommunicationException(StringResources.Language.SiemensReadLengthCannotLargerThan19);
        }

        var num = s7Addresses.Length;
        var array = new byte[19 + num * 12];
        array[0] = 3;
        array[1] = 0;
        array[2] = (byte)(array.Length / 256);
        array[3] = (byte)(array.Length % 256);
        array[4] = 2;
        array[5] = 240;
        array[6] = 128;
        array[7] = 50;
        array[8] = 1;
        array[9] = 0;
        array[10] = 0;
        array[11] = BitConverter.GetBytes(msgId)[1];
        array[12] = BitConverter.GetBytes(msgId)[0];
        array[13] = (byte)((array.Length - 17) / 256);
        array[14] = (byte)((array.Length - 17) % 256);
        array[15] = 0;
        array[16] = 0;
        array[17] = 4;
        array[18] = (byte)num;
        for (var i = 0; i < num; i++)
        {
            array[19 + i * 12] = 18;
            array[20 + i * 12] = 10;
            array[21 + i * 12] = 16;
            if (s7Addresses[i].DataCode == 30 || s7Addresses[i].DataCode == 31)
            {
                array[22 + i * 12] = s7Addresses[i].DataCode;
                array[23 + i * 12] = (byte)(s7Addresses[i].Length / 2 / 256);
                array[24 + i * 12] = (byte)(s7Addresses[i].Length / 2 % 256);
            }
            else if (s7Addresses[i].DataCode == 6 | s7Addresses[i].DataCode == 7)
            {
                array[22 + i * 12] = 4;
                array[23 + i * 12] = (byte)(s7Addresses[i].Length / 2 / 256);
                array[24 + i * 12] = (byte)(s7Addresses[i].Length / 2 % 256);
            }
            else
            {
                array[22 + i * 12] = 2;
                array[23 + i * 12] = (byte)(s7Addresses[i].Length / 256);
                array[24 + i * 12] = (byte)(s7Addresses[i].Length % 256);
            }
            array[25 + i * 12] = (byte)(s7Addresses[i].DbBlock / 256);
            array[26 + i * 12] = (byte)(s7Addresses[i].DbBlock % 256);
            array[27 + i * 12] = s7Addresses[i].DataCode;
            array[28 + i * 12] = (byte)(s7Addresses[i].AddressStart / 256 / 256 % 256);
            array[29 + i * 12] = (byte)(s7Addresses[i].AddressStart / 256 % 256);
            array[30 + i * 12] = (byte)(s7Addresses[i].AddressStart % 256);
        }
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 生成一个位读取数据指令头的通用方法。
    /// </summary>
    /// <param name="address">起始地址，例如M100.0，I0.1，Q0.1，DB2.100.2。
    /// </param>
    /// <param name="msgId">message id informaion</param>
    /// <returns>包含结果对象的报文。</returns>
    private static OperateResult<byte[]> BuildBitReadCommand(string address, int msgId)
    {
        var operateResult = S7AddressData.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[31];
        array[0] = 3;
        array[1] = 0;
        array[2] = (byte)(array.Length / 256);
        array[3] = (byte)(array.Length % 256);
        array[4] = 2;
        array[5] = 240;
        array[6] = 128;
        array[7] = 50;
        array[8] = 1;
        array[9] = 0;
        array[10] = 0;
        array[11] = BitConverter.GetBytes(msgId)[1];
        array[12] = BitConverter.GetBytes(msgId)[0];
        array[13] = (byte)((array.Length - 17) / 256);
        array[14] = (byte)((array.Length - 17) % 256);
        array[15] = 0;
        array[16] = 0;
        array[17] = 4;
        array[18] = 1;
        array[19] = 18;
        array[20] = 10;
        array[21] = 16;
        array[22] = 1;
        array[23] = 0;
        array[24] = 1;
        array[25] = (byte)(operateResult.Content!.DbBlock / 256);
        array[26] = (byte)(operateResult.Content.DbBlock % 256);
        array[27] = operateResult.Content.DataCode;
        array[28] = (byte)(operateResult.Content.AddressStart / 256 / 256 % 256);
        array[29] = (byte)(operateResult.Content.AddressStart / 256 % 256);
        array[30] = (byte)(operateResult.Content.AddressStart % 256);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 生成一个写入字节数据的指令。
    /// </summary>
    /// <param name="s7Address">起始地址，示例M100,I100,Q100,DB1.100</param>
    /// <param name="data">原始的字节数据</param>
    /// <param name="msgId">message id informaion</param>
    /// <returns>包含结果对象的报文</returns>
    private static OperateResult<byte[]> BuildWriteByteCommand(S7AddressData s7Address, byte[] data, int msgId)
    {
        return BuildWriteByteCommand([s7Address], [data], msgId);
    }

    private static void WriteS7AddressToStream(MemoryStream ms, S7AddressData add, byte writeType, int dataLen)
    {
        ms.WriteByte(18);
        ms.WriteByte(10);
        ms.WriteByte(16);
        if (add.DataCode == 6 || add.DataCode == 7)
        {
            ms.WriteByte(4);
            ms.WriteByte(BitConverter.GetBytes(dataLen / 2)[1]);
            ms.WriteByte(BitConverter.GetBytes(dataLen / 2)[0]);
        }
        else
        {
            ms.WriteByte(writeType);
            ms.WriteByte(BitConverter.GetBytes(dataLen)[1]);
            ms.WriteByte(BitConverter.GetBytes(dataLen)[0]);
        }
        ms.WriteByte(BitConverter.GetBytes(add.DbBlock)[1]);
        ms.WriteByte(BitConverter.GetBytes(add.DbBlock)[0]);
        ms.WriteByte(add.DataCode);
        ms.WriteByte(BitConverter.GetBytes(add.AddressStart)[2]);
        ms.WriteByte(BitConverter.GetBytes(add.AddressStart)[1]);
        ms.WriteByte(BitConverter.GetBytes(add.AddressStart)[0]);
    }

    private static OperateResult<byte[]> BuildWriteByteCommand(S7AddressData[] s7Address, List<byte[]> data, int msgId)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write([3, 0, 0, 0, 2, 240, 128]);
        memoryStream.WriteByte(50);
        memoryStream.WriteByte(1);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(msgId)[1]);
        memoryStream.WriteByte(BitConverter.GetBytes(msgId)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(s7Address.Length * 12 + 2)[1]);
        memoryStream.WriteByte(BitConverter.GetBytes(s7Address.Length * 12 + 2)[0]);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(5);
        memoryStream.WriteByte((byte)s7Address.Length);
        for (var i = 0; i < s7Address.Length; i++)
        {
            WriteS7AddressToStream(memoryStream, s7Address[i], 2, data[i] != null ? data[i].Length : 0);
        }
        var num = (int)memoryStream.Length;
        for (var j = 0; j < data.Count; j++)
        {
            memoryStream.WriteByte(0);
            memoryStream.WriteByte(4);
            if (data[j] != null)
            {
                memoryStream.WriteByte(BitConverter.GetBytes(data[j].Length * 8)[1]);
                memoryStream.WriteByte(BitConverter.GetBytes(data[j].Length * 8)[0]);
                memoryStream.Write(data[j]);
                if (j < data.Count - 1 && data[j].Length % 2 == 1)
                {
                    memoryStream.WriteByte(0);
                }
            }
            else
            {
                memoryStream.WriteByte(0);
                memoryStream.WriteByte(0);
            }
        }
        var array = memoryStream.ToArray();
        array[2] = (byte)(array.Length / 256);
        array[3] = (byte)(array.Length % 256);
        array[15] = BitConverter.GetBytes(array.Length - num)[1];
        array[16] = BitConverter.GetBytes(array.Length - num)[0];
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 生成一个写入位数据的指令。
    /// </summary>
    /// <param name="address">起始地址，示例M100,I100,Q100,DB1.100</param>
    /// <param name="data">是否通断</param>
    /// <param name="msgId">message id informaion</param>
    /// <returns>包含结果对象的报文</returns>
    private static OperateResult<byte[]> BuildWriteBitCommand(string address, bool data, int msgId)
    {
        var operateResult = S7AddressData.ParseFrom(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[1] { (byte)(data ? 1 : 0) };
        var array2 = new byte[35 + array.Length];
        array2[0] = 3;
        array2[1] = 0;
        array2[2] = (byte)((35 + array.Length) / 256);
        array2[3] = (byte)((35 + array.Length) % 256);
        array2[4] = 2;
        array2[5] = 240;
        array2[6] = 128;
        array2[7] = 50;
        array2[8] = 1;
        array2[9] = 0;
        array2[10] = 0;
        array2[11] = BitConverter.GetBytes(msgId)[1];
        array2[12] = BitConverter.GetBytes(msgId)[0];
        array2[13] = 0;
        array2[14] = 14;
        array2[15] = (byte)((4 + array.Length) / 256);
        array2[16] = (byte)((4 + array.Length) % 256);
        array2[17] = 5;
        array2[18] = 1;
        array2[19] = 18;
        array2[20] = 10;
        array2[21] = 16;
        array2[22] = 1;
        array2[23] = (byte)(array.Length / 256);
        array2[24] = (byte)(array.Length % 256);
        array2[25] = (byte)(operateResult.Content!.DbBlock / 256);
        array2[26] = (byte)(operateResult.Content.DbBlock % 256);
        array2[27] = operateResult.Content.DataCode;
        array2[28] = (byte)(operateResult.Content.AddressStart / 256 / 256);
        array2[29] = (byte)(operateResult.Content.AddressStart / 256);
        array2[30] = (byte)(operateResult.Content.AddressStart % 256);
        if (operateResult.Content.DataCode == 28)
        {
            array2[31] = 0;
            array2[32] = 9;
        }
        else
        {
            array2[31] = 0;
            array2[32] = 3;
        }
        array2[33] = (byte)(array.Length / 256);
        array2[34] = (byte)(array.Length % 256);
        array.CopyTo(array2, 35);
        return OperateResult.CreateSuccessResult(array2);
    }

    private static OperateResult<byte[]> AnalysisReadByte(S7AddressData[] s7Addresses, byte[] content)
    {
        try
        {
            var num = 0;
            for (var i = 0; i < s7Addresses.Length; i++)
            {
                num = s7Addresses[i].DataCode != 31 && s7Addresses[i].DataCode != 30 ? num + s7Addresses[i].Length : num + s7Addresses[i].Length * 2;
            }
            if (content.Length >= 21 && content[20] == s7Addresses.Length)
            {
                var array = new byte[num];
                var num2 = 0;
                var num3 = 0;
                for (var j = 21; j < content.Length; j++)
                {
                    if (j + 1 >= content.Length)
                    {
                        continue;
                    }
                    if (content[j] == byte.MaxValue && content[j + 1] == 4)
                    {
                        Array.Copy(content, j + 4, array, num3, s7Addresses[num2].Length);
                        j += s7Addresses[num2].Length + 3;
                        num3 += s7Addresses[num2].Length;
                        num2++;
                    }
                    else if (content[j] == byte.MaxValue && content[j + 1] == 9)
                    {
                        var num4 = content[j + 2] * 256 + content[j + 3];
                        if (num4 % 3 == 0)
                        {
                            for (var k = 0; k < num4 / 3; k++)
                            {
                                Array.Copy(content, j + 5 + 3 * k, array, num3, 2);
                                num3 += 2;
                            }
                        }
                        else
                        {
                            for (var l = 0; l < num4 / 5; l++)
                            {
                                Array.Copy(content, j + 7 + 5 * l, array, num3, 2);
                                num3 += 2;
                            }
                        }
                        j += num4 + 4;
                        num2++;
                    }
                    else
                    {
                        if (content[j] == 5 && content[j + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[j], StringResources.Language.SiemensReadLengthOverPlcAssign);
                        }
                        if (content[j] == 6 && content[j + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[j], StringResources.Language.SiemensError0006);
                        }
                        if (content[j] == 10 && content[j + 1] == 0)
                        {
                            return new OperateResult<byte[]>(content[j], StringResources.Language.SiemensError000A);
                        }
                    }
                }
                return OperateResult.CreateSuccessResult(array);
            }
            return new OperateResult<byte[]>(StringResources.Language.SiemensDataLengthCheckFailed + " Msg:" + content.ToHexString(' '));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("AnalysisReadByte failed: " + ex.Message + Environment.NewLine + " Msg:" + content.ToHexString(' '));
        }
    }

    private static OperateResult AnalysisWrite(byte[] content)
    {
        try
        {
            if (content != null && content.Length >= 22)
            {
                int num = content[20];
                for (var i = 0; i < num; i++)
                {
                    var b = content[21 + i];
                    switch (b)
                    {
                        case 5:
                            return new OperateResult(b, StringResources.Language.SiemensReadLengthOverPlcAssign);
                        case 6:
                            return new OperateResult(b, StringResources.Language.SiemensError0006);
                        case 10:
                            return new OperateResult(b, StringResources.Language.SiemensError000A);
                        default:
                            return new OperateResult(b, StringResources.Language.SiemensWriteError + b + " Msg:" + content.ToHexString(' '));
                        case byte.MaxValue:
                            break;
                    }
                }
                return OperateResult.CreateSuccessResult();
            }
            return new OperateResult(StringResources.Language.UnknownError + " Msg:" + content.ToHexString(' '));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("AnalysisWrite failed: " + ex.Message + Environment.NewLine + " Msg:" + content.ToHexString(' '));
        }
    }

    private OperateResult CheckStartResult(byte[] content)
    {
        if (content == null || content.Length < 19)
        {
            return new OperateResult("Receive error length < 19");
        }
        if (content[19] != 40)
        {
            return new OperateResult("Can not start PLC");
        }
        if (content[20] != 2)
        {
            return new OperateResult("Can not start PLC");
        }
        return OperateResult.CreateSuccessResult();
    }

    private OperateResult CheckStopResult(byte[] content)
    {
        if (content == null || content.Length < 19)
        {
            return new OperateResult("Receive error length < 19");
        }
        if (content[19] != 41)
        {
            return new OperateResult("Can not stop PLC");
        }
        if (content[20] != 7)
        {
            return new OperateResult("Can not stop PLC");
        }
        return OperateResult.CreateSuccessResult();
    }

}
