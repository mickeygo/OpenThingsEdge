using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Common.Extensions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯协议，采用A兼容1E帧协议实现，使用ASCII码通讯，请根据实际型号来进行选取。
/// </summary>
public class MelsecA1EAsciiNet : DeviceTcpNet
{
    /// <summary>
    /// PLC编号，默认为0xFF。
    /// </summary>
    public byte PLCNumber { get; set; } = byte.MaxValue;

    /// <summary>
    /// 指定ip地址和端口来来实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    /// <param name="options">配置选项</param>
    public MelsecA1EAsciiNet(string ipAddress, int port, DeviceTcpNetOptions? options = null) : base(ipAddress, port, options)
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecA1EAsciiMessage();
    }

    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = BuildReadCommand(address, length, isBit: false, PLCNumber);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var array = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            var check = CheckResponseLegal(read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(check);
            }
            var extra = ExtractActualData(read.Content, isBit: false);
            if (!extra.IsSuccess)
            {
                return extra;
            }
            array.AddRange(extra.Content);
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        if (address.IndexOf('.') > 0)
        {
            return await CommHelper.ReadBoolAsync(this, address, length).ConfigureAwait(false);
        }
        var command = BuildReadCommand(address, length, isBit: true, PLCNumber);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(command);
        }
        var array = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await ReadFromCoreServerAsync(command.Content[i]).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            var check = CheckResponseLegal(read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(check);
            }
            var extract = ExtractActualData(read.Content, isBit: true);
            if (!extract.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(extract);
            }
            array.AddRange(extract.Content);
        }
        return OperateResult.CreateSuccessResult(array.Select((m) => m == 1).Take(length).ToArray());
    }

    public override async Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        var command = BuildWriteWordCommand(address, data, PLCNumber);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = CheckResponseLegal(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult();
    }

    public override async Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        var command = BuildWriteBoolCommand(address, values, PLCNumber);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        return CheckResponseLegal(read.Content);
    }

    /// <summary>
    /// 根据类型地址长度确认需要读取的指令头
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">长度</param>
    /// <param name="isBit">指示是否按照位成批的读出</param>
    /// <param name="plcNumber">PLC编号</param>
    /// <returns>带有成功标志的指令数据</returns>
    private static OperateResult<List<byte[]>> BuildReadCommand(string address, ushort length, bool isBit, byte plcNumber)
    {
        var operateResult = MelsecHelper.McA1EAnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var value = !isBit ? (byte)1 : (byte)0;
        var array = CollectionUtils.SplitIntegerToArray(length, isBit ? 256 : 64);
        var list = new List<byte[]>();
        for (var i = 0; i < array.Length; i++)
        {
            var array2 = new byte[24]
            {
                ByteExtensions.BuildAsciiBytesFrom(value)[0],
                ByteExtensions.BuildAsciiBytesFrom(value)[1],
                ByteExtensions.BuildAsciiBytesFrom(plcNumber)[0],
                ByteExtensions.BuildAsciiBytesFrom(plcNumber)[1],
                48,
                48,
                48,
                65,
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[1],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[1],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0],
                ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1],
                0,
                0,
                0,
                0
            };
            var num = array[i];
            if (num == 256)
            {
                num = 0;
            }
            array2[20] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(num % 256)[0])[0];
            array2[21] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(num % 256)[0])[1];
            array2[22] = 48;
            array2[23] = 48;
            list.Add(array2);
            operateResult.Content2 += array[i];
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 根据类型地址以及需要写入的数据来生成指令头
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">数据值</param>
    /// <param name="plcNumber">PLC编号</param>
    /// <returns>带有成功标志的指令数据</returns>
    public static OperateResult<byte[]> BuildWriteWordCommand(string address, byte[] value, byte plcNumber)
    {
        var operateResult = MelsecHelper.McA1EAnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        value = MelsecHelper.TransByteArrayToAsciiByteArray(value);
        var array = new byte[24 + value.Length];
        array[0] = 48;
        array[1] = 51;
        array[2] = ByteExtensions.BuildAsciiBytesFrom(plcNumber)[0];
        array[3] = ByteExtensions.BuildAsciiBytesFrom(plcNumber)[1];
        array[4] = 48;
        array[5] = 48;
        array[6] = 48;
        array[7] = 65;
        array[8] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[0];
        array[9] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[1];
        array[10] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[0];
        array[11] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[1];
        array[12] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0];
        array[13] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1];
        array[14] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0];
        array[15] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1];
        array[16] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0];
        array[17] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1];
        array[18] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0];
        array[19] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1];
        array[20] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length / 4)[0])[0];
        array[21] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length / 4)[0])[1];
        array[22] = 48;
        array[23] = 48;
        value.CopyTo(array, 24);
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 根据类型地址以及需要写入的数据来生成指令头
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="value">数据值</param>
    /// <param name="plcNumber">PLC编号</param>
    /// <returns>带有成功标志的指令数据</returns>
    private static OperateResult<byte[]> BuildWriteBoolCommand(string address, bool[] value, byte plcNumber)
    {
        var operateResult = MelsecHelper.McA1EAnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var array = value.Select((m) => (byte)(m ? 49 : 48)).ToArray();
        if (array.Length % 2 == 1)
        {
            array = CollectionUtils.SpliceArray(array, new byte[1] { 48 });
        }
        var array2 = new byte[24 + array.Length];
        array2[0] = 48;
        array2[1] = 50;
        array2[2] = ByteExtensions.BuildAsciiBytesFrom(plcNumber)[0];
        array2[3] = ByteExtensions.BuildAsciiBytesFrom(plcNumber)[1];
        array2[4] = 48;
        array2[5] = 48;
        array2[6] = 48;
        array2[7] = 65;
        array2[8] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[0];
        array2[9] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[1])[1];
        array2[10] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[0];
        array2[11] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content1.DataCode)[0])[1];
        array2[12] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[0];
        array2[13] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[3])[1];
        array2[14] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[0];
        array2[15] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[2])[1];
        array2[16] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[0];
        array2[17] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[1])[1];
        array2[18] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[0];
        array2[19] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(operateResult.Content2)[0])[1];
        array2[20] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length)[0])[0];
        array2[21] = ByteExtensions.BuildAsciiBytesFrom(BitConverter.GetBytes(value.Length)[0])[1];
        array2[22] = 48;
        array2[23] = 48;
        array.CopyTo(array2, 24);
        return OperateResult.CreateSuccessResult(array2);
    }

    /// <summary>
    /// 检测反馈的消息是否合法
    /// </summary>
    /// <param name="response">接收的报文</param>
    /// <returns>是否成功</returns>
    private static OperateResult CheckResponseLegal(byte[] response)
    {
        if (response.Length < 4)
        {
            return new OperateResult(StringResources.Language.ReceiveDataLengthTooShort);
        }
        if (response[2] == 48 && response[3] == 48)
        {
            return OperateResult.CreateSuccessResult();
        }
        if (response[2] == 53 && response[3] == 66)
        {
            return new OperateResult(Convert.ToInt32(Encoding.ASCII.GetString(response, 4, 2), 16), StringResources.Language.MelsecPleaseReferToManualDocument);
        }
        return new OperateResult(Convert.ToInt32(Encoding.ASCII.GetString(response, 2, 2), 16), StringResources.Language.MelsecPleaseReferToManualDocument);
    }

    /// <summary>
    /// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
    /// </summary>
    /// <param name="response">反馈的数据内容</param>
    /// <param name="isBit">是否位读取</param>
    /// <returns>解析后的结果对象</returns>
    private static OperateResult<byte[]> ExtractActualData(byte[] response, bool isBit)
    {
        if (isBit)
        {
            return OperateResult.CreateSuccessResult((from m in response.RemoveBegin(4)
                                                      select m != 48 ? (byte)1 : (byte)0).ToArray());
        }
        return OperateResult.CreateSuccessResult(MelsecHelper.TransAsciiByteArrayToByteArray(response.RemoveBegin(4)));
    }

    public override string ToString()
    {
        return $"MelsecA1ENet[{Host}:{Port}]";
    }
}
