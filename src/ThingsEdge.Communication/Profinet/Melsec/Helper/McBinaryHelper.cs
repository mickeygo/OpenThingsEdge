using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 三菱PLC，二进制的辅助类对象
/// </summary>
public class McBinaryHelper
{
    /// <summary>
    /// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
    /// </summary>
    /// <param name="mcCore">MC协议的核心报文</param>
    /// <param name="mc">MC接口的PLC信息</param>
    /// <returns>原始报文信息</returns>
    public static byte[] PackMcCommand(IReadWriteMc mc, byte[] mcCore)
    {
        var array = new byte[11 + mcCore.Length];
        array[0] = 80;
        array[1] = 0;
        array[2] = mc.NetworkNumber;
        array[3] = mc.PLCNumber;
        array[4] = BitConverter.GetBytes(mc.TargetIOStation)[0];
        array[5] = BitConverter.GetBytes(mc.TargetIOStation)[1];
        array[6] = mc.NetworkStationNumber;
        array[7] = (byte)((array.Length - 9) % 256);
        array[8] = (byte)((array.Length - 9) / 256);
        array[9] = 10;
        array[10] = 0;
        mcCore.CopyTo(array, 11);
        return array;
    }

    /// <summary>
    /// 检查从MC返回的数据是否是合法的。
    /// </summary>
    /// <param name="content">数据内容</param>
    /// <returns>是否合法</returns>
    public static OperateResult CheckResponseContentHelper(byte[] content)
    {
        if (content == null || content.Length < 11)
        {
            return new OperateResult(StringResources.Language.ReceiveDataLengthTooShort + "11, Content: " + content.ToHexString(' '));
        }
        var num = BitConverter.ToUInt16(content, 9);
        if (num != 0)
        {
            return new OperateResult<byte[]>(num, MelsecHelper.GetErrorDescription(num));
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从三菱地址，是否位读取进行创建读取的MC的核心报文<br />
    /// From the Mitsubishi address, whether to read the core message of the MC for creating and reading
    /// </summary>
    /// <param name="isBit">是否进行了位读取操作</param>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildReadMcCoreCommand(McAddressData addressData, bool isBit)
    {
        return new byte[10]
        {
            1,
            4,
            (byte)(isBit ? 1 : 0),
            0,
            BitConverter.GetBytes(addressData.AddressStart)[0],
            BitConverter.GetBytes(addressData.AddressStart)[1],
            BitConverter.GetBytes(addressData.AddressStart)[2],
            (byte)addressData.McDataType.DataCode,
            (byte)(addressData.Length % 256),
            (byte)(addressData.Length / 256)
        };
    }

    /// <summary>
    /// 以字为单位，创建数据写入的核心报文
    /// </summary>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <param name="value">实际的原始数据信息</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildWriteWordCoreCommand(McAddressData addressData, byte[] value)
    {
        if (value == null)
        {
            value = new byte[0];
        }
        var array = new byte[10 + value.Length];
        array[0] = 1;
        array[1] = 20;
        array[2] = 0;
        array[3] = 0;
        array[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
        array[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
        array[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
        array[7] = (byte)addressData.McDataType.DataCode;
        array[8] = (byte)(value.Length / 2 % 256);
        array[9] = (byte)(value.Length / 2 / 256);
        value.CopyTo(array, 10);
        return array;
    }

    /// <summary>
    /// 以位为单位，创建数据写入的核心报文
    /// </summary>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <param name="value">原始的bool数组数据</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildWriteBitCoreCommand(McAddressData addressData, bool[] value)
    {
        if (value == null)
        {
            value = new bool[0];
        }
        var array = MelsecHelper.TransBoolArrayToByteData(value);
        var array2 = new byte[10 + array.Length];
        array2[0] = 1;
        array2[1] = 20;
        array2[2] = 1;
        array2[3] = 0;
        array2[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
        array2[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
        array2[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
        array2[7] = (byte)addressData.McDataType.DataCode;
        array2[8] = (byte)(value.Length % 256);
        array2[9] = (byte)(value.Length / 256);
        array.CopyTo(array2, 10);
        return array2;
    }

    /// <summary>
    /// 从三菱扩展地址，是否位读取进行创建读取的MC的核心报文
    /// </summary>
    /// <param name="isBit">是否进行了位读取操作</param>
    /// <param name="extend">扩展指定</param>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildReadMcCoreExtendCommand(McAddressData addressData, ushort extend, bool isBit)
    {
        return new byte[17]
        {
            1,
            4,
            (byte)(isBit ? 129 : 128),
            0,
            0,
            0,
            BitConverter.GetBytes(addressData.AddressStart)[0],
            BitConverter.GetBytes(addressData.AddressStart)[1],
            BitConverter.GetBytes(addressData.AddressStart)[2],
            (byte)addressData.McDataType.DataCode,
            0,
            0,
            BitConverter.GetBytes(extend)[0],
            BitConverter.GetBytes(extend)[1],
            249,
            (byte)(addressData.Length % 256),
            (byte)(addressData.Length / 256)
        };
    }

    /// <summary>
    /// 按字为单位随机读取的指令创建
    /// </summary>
    /// <param name="address">地址数组</param>
    /// <returns>指令</returns>
    public static byte[] BuildReadRandomWordCommand(McAddressData[] address)
    {
        var array = new byte[6 + address.Length * 4];
        array[0] = 3;
        array[1] = 4;
        array[2] = 0;
        array[3] = 0;
        array[4] = (byte)address.Length;
        array[5] = 0;
        for (var i = 0; i < address.Length; i++)
        {
            array[i * 4 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];
            array[i * 4 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
            array[i * 4 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
            array[i * 4 + 9] = (byte)address[i].McDataType.DataCode;
        }
        return array;
    }

    /// <summary>
    /// 随机读取的指令创建
    /// </summary>
    /// <param name="address">地址数组</param>
    /// <returns>指令</returns>
    public static byte[] BuildReadRandomCommand(McAddressData[] address)
    {
        var array = new byte[6 + address.Length * 6];
        array[0] = 6;
        array[1] = 4;
        array[2] = 0;
        array[3] = 0;
        array[4] = (byte)address.Length;
        array[5] = 0;
        for (var i = 0; i < address.Length; i++)
        {
            array[i * 6 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];
            array[i * 6 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
            array[i * 6 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
            array[i * 6 + 9] = (byte)address[i].McDataType.DataCode;
            array[i * 6 + 10] = (byte)(address[i].Length % 256);
            array[i * 6 + 11] = (byte)(address[i].Length / 256);
        }
        return array;
    }

    /// <summary>
    /// 创建批量读取标签的报文数据信息
    /// </summary>
    /// <param name="tags">标签名</param>
    /// <param name="lengths">长度信息</param>
    /// <returns>报文名称</returns>
    public static byte[] BuildReadTag(string[] tags, ushort[] lengths)
    {
        if (tags.Length != lengths.Length)
        {
            throw new Exception(StringResources.Language.TwoParametersLengthIsNotSame);
        }
        var memoryStream = new MemoryStream();
        memoryStream.WriteByte(26);
        memoryStream.WriteByte(4);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(BitConverter.GetBytes(tags.Length)[0]);
        memoryStream.WriteByte(BitConverter.GetBytes(tags.Length)[1]);
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        for (var i = 0; i < tags.Length; i++)
        {
            var bytes = Encoding.Unicode.GetBytes(tags[i]);
            memoryStream.WriteByte(BitConverter.GetBytes(bytes.Length / 2)[0]);
            memoryStream.WriteByte(BitConverter.GetBytes(bytes.Length / 2)[1]);
            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.WriteByte(1);
            memoryStream.WriteByte(0);
            memoryStream.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[0]);
            memoryStream.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[1]);
        }
        var result = memoryStream.ToArray();
        memoryStream.Dispose();
        return result;
    }

    /// <summary>
    /// 读取本站缓冲寄存器的数据信息，需要指定寄存器的地址，和读取的长度
    /// </summary>
    /// <param name="address">寄存器的地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>结果内容</returns>
    public static OperateResult<byte[]> BuildReadMemoryCommand(string address, ushort length)
    {
        try
        {
            var value = uint.Parse(address);
            return OperateResult.CreateSuccessResult(new byte[10]
            {
                19,
                6,
                0,
                0,
                BitConverter.GetBytes(value)[0],
                BitConverter.GetBytes(value)[1],
                BitConverter.GetBytes(value)[2],
                BitConverter.GetBytes(value)[3],
                (byte)(length % 256),
                (byte)(length / 256)
            });
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 构建读取智能模块的命令，需要指定模块编号，起始地址，读取的长度，注意，该长度以字节为单位。
    /// </summary>
    /// <param name="module">模块编号</param>
    /// <param name="address">智能模块的起始地址</param>
    /// <param name="length">读取的字长度</param>
    /// <returns>报文的结果内容</returns>
    public static OperateResult<byte[]> BuildReadSmartModule(ushort module, string address, ushort length)
    {
        try
        {
            var value = uint.Parse(address);
            return OperateResult.CreateSuccessResult(new byte[12]
            {
                1,
                6,
                0,
                0,
                BitConverter.GetBytes(value)[0],
                BitConverter.GetBytes(value)[1],
                BitConverter.GetBytes(value)[2],
                BitConverter.GetBytes(value)[3],
                (byte)(length % 256),
                (byte)(length / 256),
                BitConverter.GetBytes(module)[0],
                BitConverter.GetBytes(module)[1]
            });
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <summary>
    /// 解析出标签读取的数据内容
    /// </summary>
    /// <param name="content">返回的数据信息</param>
    /// <returns>解析结果</returns>
    public static OperateResult<byte[]> ExtraTagData(byte[] content)
    {
        try
        {
            int num = BitConverter.ToUInt16(content, 0);
            var num2 = 2;
            var list = new List<byte>(20);
            for (var i = 0; i < num; i++)
            {
                int num3 = BitConverter.ToUInt16(content, num2 + 2);
                list.AddRange(SoftBasic.ArraySelectMiddle(content, num2 + 4, num3));
                num2 += 4 + num3;
            }
            return OperateResult.CreateSuccessResult(list.ToArray());
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message + " Source:" + SoftBasic.ByteToHexString(content, ' '));
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.IReadWriteMc.ExtractActualData(System.Byte[],System.Boolean)" />
    public static byte[] ExtractActualDataHelper(byte[] response, bool isBit)
    {
        if (response == null || response.Length == 0)
        {
            return response;
        }
        if (isBit)
        {
            var array = new byte[response.Length * 2];
            for (var i = 0; i < response.Length; i++)
            {
                if ((response[i] & 0x10) == 16)
                {
                    array[i * 2] = 1;
                }
                if ((response[i] & 1) == 1)
                {
                    array[i * 2 + 1] = 1;
                }
            }
            return array;
        }
        return response;
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取PLC的标签信息，需要传入标签的名称，读取的字长度，标签举例：A; label[1]; bbb[10,10,10]<br />
    /// <b>[Authorization]</b> To read the label information of the PLC, you need to pass in the name of the label, 
    /// the length of the word read, and an example of the label: A; label [1]; bbb [10,10,10]
    /// </summary>
    /// <param name="mc">MC协议通信对象</param>
    /// <param name="tags">标签名</param>
    /// <param name="length">读取长度</param>
    /// <returns>是否成功</returns>
    /// <remarks>
    ///  不可以访问局部标签。<br />
    ///  不可以访问通过GX Works2设置的全局标签。<br />
    ///  为了访问全局标签，需要通过GX Works3的全局标签设置编辑器将“来自于外部设备的访问”的设置项目置为有效。(默认为无效。)<br />
    ///  以ASCII代码进行数据通信时，由于需要从UTF-16将标签名转换为ASCII代码，因此报文容量将增加
    /// </remarks>
    public static OperateResult<byte[]> ReadTags(IReadWriteMc mc, string[] tags, ushort[] length)
    {
        var send = BuildReadTag(tags, length);
        OperateResult<byte[]> operateResult = mc.ReadFromCoreServer(send);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return ExtraTagData(mc.ExtractActualData(operateResult.Content, isBit: false));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McBinaryHelper.ReadTags(HslCommunication.Profinet.Melsec.Helper.IReadWriteMc,System.String[],System.UInt16[])" />
    public static async Task<OperateResult<byte[]>> ReadTagsAsync(IReadWriteMc mc, string[] tags, ushort[] length)
    {
        var coreResult = BuildReadTag(tags, length);
        var read = await mc.ReadFromCoreServerAsync(coreResult);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return ExtraTagData(mc.ExtractActualData(read.Content, isBit: false));
    }
}
