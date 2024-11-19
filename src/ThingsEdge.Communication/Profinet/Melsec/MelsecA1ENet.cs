using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Melsec;

/// <summary>
/// 三菱PLC通讯协议，采用A兼容1E帧协议实现，使用二进制码通讯，请根据实际型号来进行选取。
/// </summary>
/// <remarks>
public class MelsecA1ENet : DeviceTcpNet
{
    /// <summary>
    /// PLC编号，默认为0xFF。
    /// </summary>
    public byte PLCNumber { get; set; } = byte.MaxValue;

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public MelsecA1ENet()
    {
        WordLength = 1;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 指定ip地址和端口来来实例化一个默认的对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public MelsecA1ENet(string ipAddress, int port)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new MelsecA1EBinaryMessage();
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

    /// <summary>
    /// 批量读取<see cref="bool" />数组信息，需要指定地址和长度，地址示例M100，S100，B1A，如果是X,Y, X017就是8进制地址，Y10就是16进制地址。
    /// </summary>
    /// <remarks>
    /// 根据协议的规范，最多读取256长度的bool数组信息，如果需要读取更长的bool信息，需要按字为单位进行读取的操作。
    /// </remarks>
    /// <param name="address">数据地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的byte[]数组</returns>
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        if (address.IndexOf('.') > 0)
        {
            return await CommunicationHelper.ReadBoolAsync(this, address, length).ConfigureAwait(false);
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

    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        var command = BuildWriteWordCommand(address, values, PLCNumber);
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

    /// <summary>
    /// 批量写入 <see cref="bool" />数组数据，返回是否成功，地址示例M100，S100，B1A，如果是X,Y, X017就是8进制地址，Y10就是16进制地址
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="values">写入值</param>
    /// <returns>带有成功标识的结果类对象</returns>
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

        var b = !isBit ? (byte)1 : (byte)0;
        var array = SoftBasic.SplitIntegerToArray(length, isBit ? 256 : 64);
        var list = new List<byte[]>();
        for (var i = 0; i < array.Length; i++)
        {
            var array2 = new byte[12]
            {
                b,
                plcNumber,
                10,
                0,
                BitConverter.GetBytes(operateResult.Content2)[0],
                BitConverter.GetBytes(operateResult.Content2)[1],
                BitConverter.GetBytes(operateResult.Content2)[2],
                BitConverter.GetBytes(operateResult.Content2)[3],
                BitConverter.GetBytes(operateResult.Content1.DataCode)[0],
                BitConverter.GetBytes(operateResult.Content1.DataCode)[1],
                0,
                0
            };
            var num = array[i];
            if (num == 256)
            {
                num = 0;
            }
            array2[10] = BitConverter.GetBytes(num)[0];
            array2[11] = BitConverter.GetBytes(num)[1];
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
    private static OperateResult<byte[]> BuildWriteWordCommand(string address, byte[] value, byte plcNumber)
    {
        var operateResult = MelsecHelper.McA1EAnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        var array = new byte[12 + value.Length];
        array[0] = 3;
        array[1] = plcNumber;
        array[2] = 10;
        array[3] = 0;
        array[4] = BitConverter.GetBytes(operateResult.Content2)[0];
        array[5] = BitConverter.GetBytes(operateResult.Content2)[1];
        array[6] = BitConverter.GetBytes(operateResult.Content2)[2];
        array[7] = BitConverter.GetBytes(operateResult.Content2)[3];
        array[8] = BitConverter.GetBytes(operateResult.Content1.DataCode)[0];
        array[9] = BitConverter.GetBytes(operateResult.Content1.DataCode)[1];
        array[10] = BitConverter.GetBytes(value.Length / 2)[0];
        array[11] = BitConverter.GetBytes(value.Length / 2)[1];
        Array.Copy(value, 0, array, 12, value.Length);
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
        var array = MelsecHelper.TransBoolArrayToByteData(value);
        var array2 = new byte[12 + array.Length];
        array2[0] = 2;
        array2[1] = plcNumber;
        array2[2] = 10;
        array2[3] = 0;
        array2[4] = BitConverter.GetBytes(operateResult.Content2)[0];
        array2[5] = BitConverter.GetBytes(operateResult.Content2)[1];
        array2[6] = BitConverter.GetBytes(operateResult.Content2)[2];
        array2[7] = BitConverter.GetBytes(operateResult.Content2)[3];
        array2[8] = BitConverter.GetBytes(operateResult.Content1.DataCode)[0];
        array2[9] = BitConverter.GetBytes(operateResult.Content1.DataCode)[1];
        array2[10] = BitConverter.GetBytes(value.Length)[0];
        array2[11] = BitConverter.GetBytes(value.Length)[1];
        Array.Copy(array, 0, array2, 12, array.Length);
        return OperateResult.CreateSuccessResult(array2);
    }

    /// <summary>
    /// 检测反馈的消息是否合法
    /// </summary>
    /// <param name="response">接收的报文</param>
    /// <returns>是否成功</returns>
    private static OperateResult CheckResponseLegal(byte[] response)
    {
        if (response.Length < 2)
        {
            return new OperateResult(StringResources.Language.ReceiveDataLengthTooShort);
        }
        if (response[1] == 0)
        {
            return OperateResult.CreateSuccessResult();
        }
        if (response[1] == 91)
        {
            return new OperateResult(response[2], StringResources.Language.MelsecPleaseReferToManualDocument);
        }
        return new OperateResult(response[1], StringResources.Language.MelsecPleaseReferToManualDocument);
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
            var array = new byte[(response.Length - 2) * 2];
            for (var i = 2; i < response.Length; i++)
            {
                if ((response[i] & 0x10) == 16)
                {
                    array[(i - 2) * 2] = 1;
                }
                if ((response[i] & 1) == 1)
                {
                    array[(i - 2) * 2 + 1] = 1;
                }
            }
            return OperateResult.CreateSuccessResult(array);
        }
        return OperateResult.CreateSuccessResult(response.RemoveBegin(2));
    }

    public override string ToString()
    {
        return $"MelsecA1ENet[{IpAddress}:{Port}]";
    }
}
