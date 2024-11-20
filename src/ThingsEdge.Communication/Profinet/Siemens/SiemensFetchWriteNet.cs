using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Device;
using ThingsEdge.Communication.Core.IMessage;

namespace ThingsEdge.Communication.Profinet.Siemens;

/// <summary>
/// 使用了Fetch/Write协议来和西门子进行通讯，该种方法需要在PLC侧进行一些配置。
/// </summary>
/// <remarks>
/// 配置的参考文章地址：https://www.cnblogs.com/dathlin/p/8685855.html
/// <br />
/// 与S7协议相比较而言，本协议不支持对单个的点位的读写操作。如果读取M100.0，需要读取M100的值，然后进行提取位数据。
/// 如果需要写入位地址的数据，可以读取plc的byte值，然后进行与或非，然后写入到plc之中。
/// </remarks>
public class SiemensFetchWriteNet : DeviceTcpNet
{
    /// <summary>
    /// 实例化一个西门子的Fetch/Write协议的通讯对象。
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口</param>
    public SiemensFetchWriteNet(string ipAddress, int port) : base(ipAddress, port)
    {
        WordLength = 2;
        ByteTransform = new ReverseBytesTransform();
    }

    protected override INetMessage GetNewNetMessage()
    {
        return new FetchWriteMessage();
    }

    /// <summary>
    /// 从PLC读取数据，地址格式为I100，Q100，DB20.100，M100，T100，C100，以字节为单位。
    /// </summary>
    /// <param name="address">起始地址，格式为I100，M100，Q100，DB20.100，T100，C100</param>
    /// <param name="length">读取的数量，以字节为单位</param>
    /// <returns>带有成功标志的字节信息</returns>
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = CheckResponseContent(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(SoftBasic.ArrayRemoveBegin(read.Content, 16));
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        // TODO: [NotImplemented] SiemensFetchWriteNet -> ReadBoolAsync
        throw new NotImplementedException();
    }

    /// <summary>
    /// 将数据写入到PLC数据，地址格式为I100，Q100，DB20.100，M100，以字节为单位。
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <param name="values">要写入的实际数据</param>
    /// <returns>是否写入成功的结果对象</returns>
    public override async Task<OperateResult> WriteAsync(string address, byte[] values)
    {
        var command = BuildWriteCommand(address, values);
        if (!command.IsSuccess)
        {
            return command;
        }
        var write = await ReadFromCoreServerAsync(command.Content).ConfigureAwait(false);
        if (!write.IsSuccess)
        {
            return write;
        }
        var check = CheckResponseContent(write.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 向PLC中写入byte数据，返回是否写入成功。
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <param name="value">要写入的实际数据</param>
    /// <returns>是否写入成功的结果对象</returns>
    public Task<OperateResult> WriteAsync(string address, byte value)
    {
        return WriteAsync(address, [value]);
    }

    /// <summary>
    /// 读取指定地址的byte数据。
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <returns>byte类型的结果对象</returns>
    /// <remarks>
    /// <note type="warning">
    /// 不适用于DB块，定时器，计数器的数据读取，会提示相应的错误，读取长度必须为偶数
    /// </note>
    /// </remarks>
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1).ConfigureAwait(false));
    }
 
    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        // TODO: [NotImplemented] SiemensFetchWriteNet -> WriteAsync
        throw new NotImplementedException();
    }

    /// <summary>
    /// 计算特殊的地址信息。
    /// </summary>
    /// <param name="address">字符串信息</param>
    /// <returns>实际值</returns>
    private static int CalculateAddressStarted(string address)
    {
        if (address.IndexOf('.') < 0)
        {
            return Convert.ToInt32(address);
        }
        var array = address.Split('.');
        return Convert.ToInt32(array[0]);
    }

    private static OperateResult CheckResponseContent(byte[] content)
    {
        if (content.Length < 9)
        {
            return new OperateResult(StringResources.Language.ReceiveDataLengthTooShort + "9, Content: " + content.ToHexString(' '));
        }
        if (content[8] != 0)
        {
            return new OperateResult(content[8], StringResources.Language.SiemensWriteError + content[8]);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 解析数据地址，解析出地址类型，起始地址，DB块的地址
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <returns>解析出地址类型，起始地址，DB块的地址</returns>
    private static OperateResult<byte, int, ushort> AnalysisAddress(string address)
    {
        var operateResult = new OperateResult<byte, int, ushort>();
        try
        {
            operateResult.Content3 = 0;
            if (address[0] == 'I')
            {
                operateResult.Content1 = 3;
                operateResult.Content2 = CalculateAddressStarted(address[1..]);
            }
            else if (address[0] == 'Q')
            {
                operateResult.Content1 = 4;
                operateResult.Content2 = CalculateAddressStarted(address[1..]);
            }
            else if (address[0] == 'M')
            {
                operateResult.Content1 = 2;
                operateResult.Content2 = CalculateAddressStarted(address[1..]);
            }
            else if (address[0] == 'D' || address[..2] == "DB")
            {
                operateResult.Content1 = 1;
                var array = address.Split('.');
                if (address[1] == 'B')
                {
                    operateResult.Content3 = Convert.ToUInt16(array[0][2..]);
                }
                else
                {
                    operateResult.Content3 = Convert.ToUInt16(array[0][1..]);
                }
                if (operateResult.Content3 > 255)
                {
                    operateResult.Message = StringResources.Language.SiemensDBAddressNotAllowedLargerThan255;
                    return operateResult;
                }
                operateResult.Content2 = CalculateAddressStarted(address[(address.IndexOf('.') + 1)..]);
            }
            else if (address[0] == 'T')
            {
                operateResult.Content1 = 7;
                operateResult.Content2 = CalculateAddressStarted(address[1..]);
            }
            else
            {
                if (address[0] != 'C')
                {
                    operateResult.Message = StringResources.Language.NotSupportedDataType;
                    operateResult.Content1 = 0;
                    operateResult.Content2 = 0;
                    operateResult.Content3 = 0;
                    return operateResult;
                }
                operateResult.Content1 = 6;
                operateResult.Content2 = CalculateAddressStarted(address[1..]);
            }
        }
        catch (Exception ex)
        {
            operateResult.Message = ex.Message;
            return operateResult;
        }
        operateResult.IsSuccess = true;
        return operateResult;
    }

    /// <summary>
    /// 生成一个读取字数据指令头的通用方法。
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <param name="count">读取数据个数</param>
    /// <returns>带结果对象的报文数据</returns>
    private static OperateResult<byte[]> BuildReadCommand(string address, ushort count)
    {
        var operateResult = AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[16]
        {
            83,
            53,
            16,
            1,
            3,
            5,
            3,
            8,
            operateResult.Content1,
            (byte)operateResult.Content3,
            (byte)(operateResult.Content2 / 256),
            (byte)(operateResult.Content2 % 256),
            0,
            0,
            0,
            0
        };
        if (operateResult.Content1 == 1 || operateResult.Content1 == 6 || operateResult.Content1 == 7)
        {
            if (count % 2 != 0)
            {
                return new OperateResult<byte[]>(StringResources.Language.SiemensReadLengthMustBeEvenNumber);
            }
            array[12] = BitConverter.GetBytes(count / 2)[1];
            array[13] = BitConverter.GetBytes(count / 2)[0];
        }
        else
        {
            array[12] = BitConverter.GetBytes(count)[1];
            array[13] = BitConverter.GetBytes(count)[0];
        }
        array[14] = byte.MaxValue;
        array[15] = 2;
        return OperateResult.CreateSuccessResult(array);
    }

    /// <summary>
    /// 生成一个写入字节数据的指令
    /// </summary>
    /// <param name="address">起始地址，格式为M100,I100,Q100,DB1.100</param>
    /// <param name="data">实际的写入的内容</param>
    /// <returns>带结果对象的报文数据</returns>
    private static OperateResult<byte[]> BuildWriteCommand(string address, byte[] data)
    {
        var operateResult = AnalysisAddress(address);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }

        var array = new byte[16 + data.Length];
        array[0] = 83;
        array[1] = 53;
        array[2] = 16;
        array[3] = 1;
        array[4] = 3;
        array[5] = 3;
        array[6] = 3;
        array[7] = 8;
        array[8] = operateResult.Content1;
        array[9] = (byte)operateResult.Content3;
        array[10] = (byte)(operateResult.Content2 / 256);
        array[11] = (byte)(operateResult.Content2 % 256);
        if (operateResult.Content1 == 1 || operateResult.Content1 == 6 || operateResult.Content1 == 7)
        {
            if (data.Length % 2 != 0)
            {
                return new OperateResult<byte[]>(StringResources.Language.SiemensReadLengthMustBeEvenNumber);
            }
            array[12] = BitConverter.GetBytes(data.Length / 2)[1];
            array[13] = BitConverter.GetBytes(data.Length / 2)[0];
        }
        else
        {
            array[12] = BitConverter.GetBytes(data.Length)[1];
            array[13] = BitConverter.GetBytes(data.Length)[0];
        }
        array[14] = byte.MaxValue;
        array[15] = 2;
        Array.Copy(data, 0, array, 16, data.Length);
        return OperateResult.CreateSuccessResult(array);
    }

    public override string ToString()
    {
        return $"SiemensFetchWriteNet[{IpAddress}:{Port}]";
    }
}
