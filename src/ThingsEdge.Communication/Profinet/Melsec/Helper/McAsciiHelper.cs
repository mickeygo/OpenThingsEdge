using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 基于MC协议的ASCII格式的辅助类
/// </summary>
public class McAsciiHelper
{
    /// <summary>
    /// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
    /// </summary>
    /// <param name="mc">三菱MC协议的核心通信对象</param>
    /// <param name="mcCore">MC协议的核心报文</param>
    /// <returns>原始报文信息</returns>
    public static byte[] PackMcCommand(IReadWriteMc mc, byte[] mcCore)
    {
        var array = SoftBasic.BuildAsciiBytesFrom(mc.TargetIOStation);
        var array2 = new byte[22 + mcCore.Length];
        array2[0] = 53;
        array2[1] = 48;
        array2[2] = 48;
        array2[3] = 48;
        array2[4] = SoftBasic.BuildAsciiBytesFrom(mc.NetworkNumber)[0];
        array2[5] = SoftBasic.BuildAsciiBytesFrom(mc.NetworkNumber)[1];
        array2[6] = SoftBasic.BuildAsciiBytesFrom(mc.PLCNumber)[0];
        array2[7] = SoftBasic.BuildAsciiBytesFrom(mc.PLCNumber)[1];
        array2[8] = array[0];
        array2[9] = array[1];
        array2[10] = array[2];
        array2[11] = array[3];
        array2[12] = SoftBasic.BuildAsciiBytesFrom(mc.NetworkStationNumber)[0];
        array2[13] = SoftBasic.BuildAsciiBytesFrom(mc.NetworkStationNumber)[1];
        array2[14] = SoftBasic.BuildAsciiBytesFrom((ushort)(array2.Length - 18))[0];
        array2[15] = SoftBasic.BuildAsciiBytesFrom((ushort)(array2.Length - 18))[1];
        array2[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(array2.Length - 18))[2];
        array2[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(array2.Length - 18))[3];
        array2[18] = 48;
        array2[19] = 48;
        array2[20] = 49;
        array2[21] = 48;
        mcCore.CopyTo(array2, 22);
        return array2;
    }

    /// <summary>
    /// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
    /// </summary>
    /// <param name="response">反馈的数据内容</param>
    /// <param name="isBit">是否位读取</param>
    /// <returns>解析后的结果对象</returns>
    public static byte[] ExtractActualDataHelper(byte[] response, bool isBit)
    {
        if (isBit)
        {
            return response.Select((m) => m != 48 ? (byte)1 : (byte)0).ToArray();
        }
        return MelsecHelper.TransAsciiByteArrayToByteArray(response);
    }

    /// <summary>
    /// 检查反馈的内容是否正确的
    /// </summary>
    /// <param name="content">MC的反馈的内容</param>
    /// <returns>是否正确</returns>
    public static OperateResult CheckResponseContent(byte[] content)
    {
        if (content == null || content.Length < 22)
        {
            return new OperateResult(StringResources.Language.ReceiveDataLengthTooShort + "22, Content: " + SoftBasic.GetAsciiStringRender(content));
        }
        var num = Convert.ToUInt16(Encoding.ASCII.GetString(content, 18, 4), 16);
        if (num != 0)
        {
            return new OperateResult(num, MelsecHelper.GetErrorDescription(num));
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 从三菱地址，是否位读取进行创建读取Ascii格式的MC的核心报文
    /// </summary>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <param name="isBit">是否进行了位读取操作</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildAsciiReadMcCoreCommand(McAddressData addressData, bool isBit)
    {
        return new byte[20]
        {
            48,
            52,
            48,
            49,
            48,
            48,
            48,
            (byte)(isBit ? 49 : 48),
            Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0],
            Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[0],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[1],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[2],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[3]
        };
    }

    /// <summary>
    /// 从三菱扩展地址，是否位读取进行创建读取的MC的核心报文
    /// </summary>
    /// <param name="isBit">是否进行了位读取操作</param>
    /// <param name="extend">扩展指定</param>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildAsciiReadMcCoreExtendCommand(McAddressData addressData, ushort extend, bool isBit)
    {
        return new byte[32]
        {
            48,
            52,
            48,
            49,
            48,
            48,
            56,
            (byte)(isBit ? 49 : 48),
            48,
            48,
            74,
            SoftBasic.BuildAsciiBytesFrom(extend)[1],
            SoftBasic.BuildAsciiBytesFrom(extend)[2],
            SoftBasic.BuildAsciiBytesFrom(extend)[3],
            48,
            48,
            48,
            Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0],
            Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4],
            MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5],
            48,
            48,
            48,
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[0],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[1],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[2],
            SoftBasic.BuildAsciiBytesFrom(addressData.Length)[3]
        };
    }

    /// <summary>
    /// 以字为单位，创建ASCII数据写入的核心报文
    /// </summary>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <param name="value">实际的原始数据信息</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildAsciiWriteWordCoreCommand(McAddressData addressData, byte[] value)
    {
        value = MelsecHelper.TransByteArrayToAsciiByteArray(value);
        var array = new byte[20 + value.Length];
        array[0] = 49;
        array[1] = 52;
        array[2] = 48;
        array[3] = 49;
        array[4] = 48;
        array[5] = 48;
        array[6] = 48;
        array[7] = 48;
        array[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
        array[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
        array[10] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
        array[11] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
        array[12] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
        array[13] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
        array[14] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
        array[15] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
        array[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[0];
        array[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[1];
        array[18] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[2];
        array[19] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[3];
        value.CopyTo(array, 20);
        return array;
    }

    /// <summary>
    /// 以位为单位，创建ASCII数据写入的核心报文
    /// </summary>
    /// <param name="addressData">三菱Mc协议的数据地址</param>
    /// <param name="value">原始的bool数组数据</param>
    /// <returns>带有成功标识的报文对象</returns>
    public static byte[] BuildAsciiWriteBitCoreCommand(McAddressData addressData, bool[] value)
    {
        if (value == null)
        {
            value = new bool[0];
        }
        var array = value.Select((m) => (byte)(m ? 49 : 48)).ToArray();
        var array2 = new byte[20 + array.Length];
        array2[0] = 49;
        array2[1] = 52;
        array2[2] = 48;
        array2[3] = 49;
        array2[4] = 48;
        array2[5] = 48;
        array2[6] = 48;
        array2[7] = 49;
        array2[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
        array2[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
        array2[10] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
        array2[11] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
        array2[12] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
        array2[13] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
        array2[14] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
        array2[15] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
        array2[16] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[0];
        array2[17] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[1];
        array2[18] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[2];
        array2[19] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[3];
        array.CopyTo(array2, 20);
        return array2;
    }

    /// <summary>
    /// 按字为单位随机读取的指令创建
    /// </summary>
    /// <param name="address">地址数组</param>
    /// <returns>指令</returns>
    public static byte[] BuildAsciiReadRandomWordCommand(McAddressData[] address)
    {
        var array = new byte[12 + address.Length * 8];
        array[0] = 48;
        array[1] = 52;
        array[2] = 48;
        array[3] = 51;
        array[4] = 48;
        array[5] = 48;
        array[6] = 48;
        array[7] = 48;
        array[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
        array[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
        array[10] = 48;
        array[11] = 48;
        for (var i = 0; i < address.Length; i++)
        {
            array[i * 8 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];
            array[i * 8 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
            array[i * 8 + 14] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];
            array[i * 8 + 15] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
            array[i * 8 + 16] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
            array[i * 8 + 17] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
            array[i * 8 + 18] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
            array[i * 8 + 19] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
        }
        return array;
    }

    /// <summary>
    /// 随机读取的指令创建
    /// </summary>
    /// <param name="address">地址数组</param>
    /// <returns>指令</returns>
    public static byte[] BuildAsciiReadRandomCommand(McAddressData[] address)
    {
        var array = new byte[12 + address.Length * 12];
        array[0] = 48;
        array[1] = 52;
        array[2] = 48;
        array[3] = 54;
        array[4] = 48;
        array[5] = 48;
        array[6] = 48;
        array[7] = 48;
        array[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
        array[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
        array[10] = 48;
        array[11] = 48;
        for (var i = 0; i < address.Length; i++)
        {
            array[i * 12 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];
            array[i * 12 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
            array[i * 12 + 14] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];
            array[i * 12 + 15] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
            array[i * 12 + 16] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
            array[i * 12 + 17] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
            array[i * 12 + 18] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
            array[i * 12 + 19] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
            array[i * 12 + 20] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[0];
            array[i * 12 + 21] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[1];
            array[i * 12 + 22] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[2];
            array[i * 12 + 23] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[3];
        }
        return array;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McBinaryHelper.BuildReadMemoryCommand(System.String,System.UInt16)" />
    public static OperateResult<byte[]> BuildAsciiReadMemoryCommand(string address, ushort length)
    {
        try
        {
            var value = uint.Parse(address);
            var array = new byte[20]
            {
                48, 54, 49, 51, 48, 48, 48, 48, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            SoftBasic.BuildAsciiBytesFrom(value).CopyTo(array, 8);
            SoftBasic.BuildAsciiBytesFrom(length).CopyTo(array, 16);
            return OperateResult.CreateSuccessResult(array);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message);
        }
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Melsec.Helper.McBinaryHelper.BuildReadSmartModule(System.UInt16,System.String,System.UInt16)" />
    public static OperateResult<byte[]> BuildAsciiReadSmartModule(ushort module, string address, ushort length)
    {
        try
        {
            var value = uint.Parse(address);
            var array = new byte[24]
            {
                48, 54, 48, 49, 48, 48, 48, 48, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0
            };
            SoftBasic.BuildAsciiBytesFrom(value).CopyTo(array, 8);
            SoftBasic.BuildAsciiBytesFrom(length).CopyTo(array, 16);
            SoftBasic.BuildAsciiBytesFrom(module).CopyTo(array, 20);
            return OperateResult.CreateSuccessResult(array);
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>(ex.Message);
        }
    }
}
