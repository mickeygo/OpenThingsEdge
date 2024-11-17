using System.Text.RegularExpressions;
using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.AllenBradley;

namespace ThingsEdge.Communication.Profinet.Omron;

/// <summary>
/// 基于连接的对象访问的CIP协议的实现，用于对Omron PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。<br />
/// The implementation of the CIP protocol based on connected object access is used to read and write tag data to Omron PLC, 
/// and read and write arrays and multidimensional arrays. For the supported data types, please refer to the API documentation manual.
/// </summary>
/// <remarks>
/// 支持普通标签的读写，类型要和标签对应上。如果标签是数组，例如 A 是 INT[0...9] 那么Read("A", 1)，返回的是10个short所有字节数组。
/// 如果需要返回10个长度的short数组，请调用 ReadInt16("A[0], 10"); 地址必须写 "A[0]"，不能写 "A" , 如需要读取结构体，参考 <see cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadStruct``1(System.String)" />
/// </remarks>
/// <example>
/// 首先说明支持的类型地址，在PLC里支持了大量的类型，有些甚至在C#里是不存在的。现在做个统一的声明
/// <list type="table">
///   <listheader>
///     <term>PLC类型</term>
///     <term>含义</term>
///     <term>代号</term>
///     <term>C# 类型</term>
///     <term>备注</term>
///   </listheader>
///   <item>
///     <term>bool</term>
///     <term>位类型数据</term>
///     <term>0xC1</term>
///     <term>bool</term>
///     <term></term>
///   </item>
///   <item>
///     <term>SINT</term>
///     <term>8位的整型</term>
///     <term>0xC2</term>
///     <term>sbyte</term>
///     <term>有符号8位很少用，HSL直接用byte</term>
///   </item>
///   <item>
///     <term>USINT</term>
///     <term>无符号8位的整型</term>
///     <term>0xC6</term>
///     <term>byte</term>
///     <term>如需要，使用<see cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />实现</term>
///   </item>
///   <item>
///     <term>BYTE</term>
///     <term>8位字符数据</term>
///     <term>0xD1</term>
///     <term>byte</term>
///     <term>如需要，使用<see cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />实现</term>
///   </item>
///   <item>
///     <term>INT</term>
///     <term>16位的整型</term>
///     <term>0xC3</term>
///     <term>short</term>
///     <term></term>
///   </item>
///   <item>
///     <term>UINT</term>
///     <term>无符号的16位整型</term>
///     <term>0xC7</term>
///     <term>ushort</term>
///     <term></term>
///   </item>
///   <item>
///     <term>DINT</term>
///     <term>32位的整型</term>
///     <term>0xC4</term>
///     <term>int</term>
///     <term></term>
///   </item>
///   <item>
///     <term>UDINT</term>
///     <term>无符号的32位整型</term>
///     <term>0xC8</term>
///     <term>uint</term>
///     <term></term>
///   </item>
///   <item>
///     <term>LINT</term>
///     <term>64位的整型</term>
///     <term>0xC5</term>
///     <term>long</term>
///     <term></term>
///   </item>
///   <item>
///     <term>ULINT</term>
///     <term>无符号的64位的整型</term>
///     <term>0xC9</term>
///     <term>ulong</term>
///     <term></term>
///   </item>
///   <item>
///     <term>REAL</term>
///     <term>单精度浮点数</term>
///     <term>0xCA</term>
///     <term>float</term>
///     <term></term>
///   </item>
///   <item>
///     <term>DOUBLE</term>
///     <term>双精度浮点数</term>
///     <term>0xCB</term>
///     <term>double</term>
///     <term></term>
///   </item>
///   <item>
///     <term>STRING</term>
///     <term>字符串数据</term>
///     <term>0xD0</term>
///     <term>string</term>
///     <term>前两个字节为字符长度</term>
///   </item>
///   <item>
///     <term>8bit string BYTE</term>
///     <term>8位的字符串</term>
///     <term>0xD1</term>
///     <term></term>
///     <term>本质是BYTE数组</term>
///   </item>
///   <item>
///     <term>16bit string WORD</term>
///     <term>16位的字符串</term>
///     <term>0xD2</term>
///     <term></term>
///     <term>本质是WORD数组，可存放中文</term>
///   </item>
///   <item>
///     <term>32bit string DWORD</term>
///     <term>32位的字符串</term>
///     <term>0xD2</term>
///     <term></term>
///     <term>本质是DWORD数组，可存放中文</term>
///   </item>
/// </list>
/// 在读写操作之前，先看看怎么实例化和连接PLC<br />
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage" title="实例化及连接示例" />
/// 现在来说明以下具体的操作细节。我们假设有如下的变量：<br />
/// CESHI_A       SINT<br />
/// CESHI_B       BYTE<br />
/// CESHI_C       INT<br />
/// CESHI_D       UINT<br />
/// CESHI_E       SINT[0..9]<br />
/// CESHI_F       BYTE[0..9]<br />
/// CESHI_G       INT[0..9]<br />
/// CESHI_H       UINT[0..9]<br />
/// CESHI_I       INT[0..511]<br />
/// CESHI_J       STRING[12]<br />
/// ToPc_ID1      ARRAY[0..99] OF STRING[20]<br />
/// CESHI_O       BOOL<br />
/// CESHI_P       BOOL[0..31]<br />
/// 对 CESHI_A 来说，读写这么操作
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage2" title="读写示例" />
/// 对于 CESHI_B 来说，写入的操作有点特殊
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage3" title="读写示例" />
/// 对于 CESHI_C, CESHI_D 来说，就是 ReadInt16(string address) , Write( string address, short value ) 和 ReadUInt16(string address) 和 Write( string address, ushort value ) 差别不大。
/// 所以我们着重来看看数组的情况，以 CESHI_G 标签为例子:<br />
/// 情况一，我想一次读取这个标签所有的字节数组（当长度满足的情况下，会一次性返回数据）
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage4" title="读写示例" />
/// 情况二，我想读取第3个数，或是第6个数开始，一共5个数
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage5" title="读写示例" />
/// 其他的数组情况都是类似的，我们来看看字符串 CESHI_J 变量
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage6" title="读写示例" />
/// 对于 bool 变量来说，就是 ReadBool("CESHI_O") 和 Write("CESHI_O", true) 操作，如果是bool数组，就不一样了
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage7" title="读写示例" />
/// 最后我们来看看结构体的操作，假设我们有个结构体<br />
/// MyData.Code     STRING(12)<br />
/// MyData.Value1   INT<br />
/// MyData.Value2   INT<br />
/// MyData.Value3   REAL<br />
/// MyData.Value4   INT<br />
/// MyData.Value5   INT<br />
/// MyData.Value6   INT[0..3]<br />
/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage8" title="结构体" />
/// 定义好后，我们再来读取就很简单了。
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage9" title="读写示例" />
/// </example>
public class OmronConnectedCipNet : NetworkConnectedCip, IReadWriteCip, IReadWriteNet
{
    /// <summary>
    /// 当前产品的型号信息<br />
    /// Model information of the current product
    /// </summary>
    public string ProductName { get; private set; }

    /// <summary>
    /// 获取或设置不通信时超时的时间，默认02，为 32 秒，设置06 时为8分多，计算方法为 (2的x次方乘以8) 的秒数<br />
    /// Get or set the timeout time when there is no communication. The default is 02, which is 32 seconds, and when 06 is set, it is more than 8 minutes. The calculation method is (2 times the power of x times 8) seconds.
    /// </summary>
    public byte ConnectionTimeoutMultiplier { get; set; } = 2;


    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public OmronConnectedCipNet()
    {
        WordLength = 2;
        ByteTransform = new RegularByteTransform();
    }

    /// <summary>
    /// 根据指定的IP及端口来实例化这个连接对象
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口号信息</param>
    public OmronConnectedCipNet(string ipAddress, int port = 44818)
        : this()
    {
        IpAddress = ipAddress;
        Port = port;
    }

    /// <inheritdoc />
    protected override byte[] GetLargeForwardOpen(ushort connectionID)
    {
        var value = TOConnectionId = (uint)(-2130837503 + connectionID);
        var array = "\r\n00 00 00 00 00 00 02 00 00 00 00 00 b2 00 34 00\r\n5b 02 20 06 24 01 0e 9c 02 00 00 80 01 00 fe 80\r\n02 00 1b 05 30 a7 2b 03 02 00 00 00 80 84 1e 00\r\ncc 07 00 42 80 84 1e 00 cc 07 00 42 a3 03 20 02\r\n24 01 2c 01".ToHexBytes();
        BitConverter.GetBytes((uint)(-2147483646 + connectionID)).CopyTo(array, 24);
        BitConverter.GetBytes(value).CopyTo(array, 28);
        BitConverter.GetBytes((ushort)(2 + connectionID)).CopyTo(array, 32);
        BitConverter.GetBytes((ushort)4105).CopyTo(array, 34);
        CommHelper.HslRandom.GetBytes(4).CopyTo(array, 36);
        array[40] = ConnectionTimeoutMultiplier;
        return array;
    }

    /// <inheritdoc />
    protected override OperateResult InitializationOnConnect()
    {
        var operateResult = base.InitializationOnConnect();
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(CommunicationPipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetAttributeAll()), hasResponseData: true, usePackAndUnpack: false);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        if (operateResult2.Content.Length > 59 && operateResult2.Content.Length >= 59 + operateResult2.Content[58])
        {
            ProductName = Encoding.UTF8.GetString(operateResult2.Content, 59, operateResult2.Content[58]);
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <inheritdoc />
    protected override async Task<OperateResult> InitializationOnConnectAsync()
    {
        var ini = await base.InitializationOnConnectAsync().ConfigureAwait(continueOnCapturedContext: false);
        if (!ini.IsSuccess)
        {
            return ini;
        }
        var read = await ReadFromCoreServerAsync(CommunicationPipe, AllenBradleyHelper.PackRequestHeader(111, SessionHandle, GetAttributeAll()), hasResponseData: true, usePackAndUnpack: false).ConfigureAwait(continueOnCapturedContext: false);
        if (!read.IsSuccess)
        {
            return read;
        }
        if (read.Content.Length > 59 && read.Content.Length >= 59 + read.Content[58])
        {
            ProductName = Encoding.UTF8.GetString(read.Content, 59, read.Content[58]);
        }
        return OperateResult.CreateSuccessResult();
    }

    private byte[] GetAttributeAll()
    {
        return "00 00 00 00 00 00 02 00 00 00 00 00 b2 00 06 00 01 02 20 01 24 01".ToHexBytes();
    }

    private OperateResult<byte[]> BuildReadCommand(string[] address, ushort[] length)
    {
        try
        {
            var list = new List<byte[]>();
            for (var i = 0; i < address.Length; i++)
            {
                list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i], isConnectedAddress: true));
            }
            return OperateResult.CreateSuccessResult(PackCommandService(list.ToArray()));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    private OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
    {
        try
        {
            return OperateResult.CreateSuccessResult(PackCommandService(AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length, isConnectedAddress: true)));
        }
        catch (Exception ex)
        {
            return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
        }
    }

    private OperateResult<byte[], ushort, bool> ReadWithType(string[] address, ushort[] length)
    {
        var operateResult = BuildReadCommand(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(operateResult);
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(operateResult2);
        }
        var operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(operateResult3);
        }
        return ExtractActualData(operateResult2.Content, isRead: true);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.ReadCipFromServer(System.Byte[][])" />
    public OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
    {
        var send = PackCommandService(cips.ToArray());
        var operateResult = ReadFromCoreServer(send);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = AllenBradleyHelper.CheckResponse(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content);
    }

    /// <summary>
    /// <b>[商业授权]</b> 读取一个结构体的对象，需要事先根据实际的数据点位定义好结构体，然后使用本方法进行读取，当结构体定义不对时，本方法将会读取失败<br />
    /// <b>[Authorization]</b> To read a structure object, you need to define the structure in advance according to the actual data points, 
    /// and then use this method to read. When the structure definition is incorrect, this method will fail to read
    /// </summary>
    /// <remarks>
    /// 本方法需要商业授权支持，具体的使用方法，参考API文档的示例代码
    /// </remarks>
    /// <example>
    /// 我们来看看结构体的操作，假设我们有个结构体<br />
    /// MyData.Code     STRING(12)<br />
    /// MyData.Value1   INT<br />
    /// MyData.Value2   INT<br />
    /// MyData.Value3   REAL<br />
    /// MyData.Value4   INT<br />
    /// MyData.Value5   INT<br />
    /// MyData.Value6   INT[0..3]<br />
    /// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage8" title="结构体" />
    /// 定义好后，我们再来读取就很简单了。
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronConnectedCipNetSample.cs" region="Usage9" title="读写示例" />
    /// </example>
    /// <typeparam name="T">结构体的类型</typeparam>
    /// <param name="address">结构体对象的地址</param>
    /// <returns>是否读取成功的对象</returns>
    public OperateResult<T> ReadStruct<T>(string address) where T : struct
    {
        var operateResult = Read(address, 1);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<T>(operateResult);
        }
        return CommHelper.ByteArrayToStruct<T>(operateResult.Content.RemoveBegin(2));
    }

    private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string[] address, ushort[] length)
    {
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(command);
        }
        var read = await ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(read);
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[], ushort, bool>(check);
        }
        return ExtractActualData(read.Content, isRead: true);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadCipFromServer(System.Byte[][])" />
    public async Task<OperateResult<byte[]>> ReadCipFromServerAsync(params byte[][] cips)
    {
        var command = PackCommandService(cips.ToArray());
        var read = await ReadFromCoreServerAsync(command);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return OperateResult.CreateSuccessResult(read.Content);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadStruct``1(System.String)" />
    public async Task<OperateResult<T>> ReadStructAsync<T>(string address) where T : struct
    {
        var read = await ReadAsync(address, 1);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<T>(read);
        }
        return CommHelper.ByteArrayToStruct<T>(read.Content.RemoveBegin(2));
    }

    /// <summary>
    /// 获取传递的最大长度的字节信息
    /// </summary>
    /// <returns>字节长度</returns>
    protected virtual int GetMaxTransferBytes()
    {
        return 1988;
    }

    private int GetLengthFromRemain(ushort dataType, int length)
    {
        if (dataType == 193 || dataType == 194 || dataType == 198 || dataType == 211)
        {
            return Math.Min(length, GetMaxTransferBytes());
        }
        if (dataType == 199 || dataType == 195)
        {
            return Math.Min(length, GetMaxTransferBytes() / 2);
        }
        if (dataType == 196 || dataType == 200 || dataType == 202)
        {
            return Math.Min(length, GetMaxTransferBytes() / 4);
        }
        return Math.Min(length, GetMaxTransferBytes() / 8);
    }

    /// <inheritdoc />
    public override OperateResult<byte[]> Read(string address, ushort length)
    {
        CommHelper.ExtractParameter(ref address, "type", 0);
        if (length == 1)
        {
            var operateResult = ReadWithType(new string[1] { address }, new ushort[1] { length });
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(operateResult);
            }
            return OperateResult.CreateSuccessResult(operateResult.Content1);
        }
        var num = 0;
        var num2 = 0;
        var format = "[{0}]";
        var list = new List<byte>();
        var match = Regex.Match(address, "\\[[0-9]+\\]$");
        if (match.Success)
        {
            address = address.Remove(match.Index, match.Length);
            num2 = int.Parse(match.Value.Substring(1, match.Value.Length - 2));
        }
        else
        {
            var match2 = Regex.Match(address, "\\[[0-9]+,[0-9]+\\]$");
            if (match2.Success)
            {
                address = address.Remove(match2.Index, match2.Length);
                var value = Regex.Matches(match2.Value, "[0-9]+")[1].Value;
                format = match2.Value.Replace(value + "]", "{0}]");
                num2 = int.Parse(value);
            }
        }
        ushort dataType = 0;
        while (num < length)
        {
            if (num == 0)
            {
                var num3 = Math.Min(length, (ushort)248);
                var operateResult2 = ReadWithType(new string[1] { address + string.Format(format, num2) }, new ushort[1] { num3 });
                if (!operateResult2.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(operateResult2);
                }
                dataType = operateResult2.Content2;
                num += num3;
                num2 += num3;
                list.AddRange(operateResult2.Content1);
            }
            else
            {
                var num4 = (ushort)GetLengthFromRemain(dataType, length - num);
                var operateResult3 = ReadWithType(new string[1] { address + string.Format(format, num2) }, new ushort[1] { num4 });
                if (!operateResult3.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(operateResult3);
                }
                num += num4;
                num2 += num4;
                list.AddRange(operateResult3.Content1);
            }
        }
        return OperateResult.CreateSuccessResult(list.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.Read(System.String[],System.UInt16[])" />
    public OperateResult<byte[]> Read(string[] address, ushort[] length)
    {
        var operateResult = ReadWithType(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content1);
    }

    /// <summary>
    /// 读取bool数据信息，如果读取的是单bool变量，就直接写变量名，如果是 bool 数组，就 <br />
    /// Read a single bool data information, if it is a single bool variable, write the variable name directly, 
    /// if it is a value of a bool array composed of int, it is always accessed with "i=" at the beginning, for example, "i=A[0]"
    /// </summary>
    /// <param name="address">节点的名称 -&gt; Name of the node </param>
    /// <param name="length">读取的数组长度信息</param>
    /// <returns>带有结果对象的结果数据 -&gt; Result data with result info </returns>
    public override OperateResult<bool[]> ReadBool(string address, ushort length)
    {
        if (length == 1 && !Regex.IsMatch(address, "\\[[0-9]+\\]$"))
        {
            var operateResult = Read(address, length);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(operateResult);
            }
            return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(operateResult.Content));
        }
        var operateResult2 = Read(address, length);
        if (!operateResult2.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(operateResult2);
        }
        return OperateResult.CreateSuccessResult(operateResult2.Content.Select((m) => m != 0).Take(length).ToArray());
    }

    /// <summary>
    /// 读取PLC的byte类型的数据<br />
    /// Read the byte type of PLC data
    /// </summary>
    /// <param name="address">节点的名称 -&gt; Name of the node </param>
    /// <returns>带有结果对象的结果数据 -&gt; Result data with result info </returns>
    public OperateResult<byte> ReadByte(string address)
    {
        return ByteTransformHelper.GetResultFromArray(Read(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.ReadTag(System.String,System.UInt16)" />
    public OperateResult<ushort, byte[]> ReadTag(string address, ushort length = 1)
    {
        var operateResult = ReadWithType(new string[1] { address }, new ushort[1] { length });
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, byte[]>(operateResult);
        }
        return OperateResult.CreateSuccessResult(operateResult.Content2, operateResult.Content1);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadPlcType(HslCommunication.Core.IReadWriteDevice)" />
    public OperateResult<string> ReadPlcType()
    {
        return OperateResult.CreateSuccessResult(ProductName);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadBool(System.String,System.UInt16)" />
    public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        OperateResult<byte[]> read;
        if (length == 1 && !Regex.IsMatch(address, "\\[[0-9]+\\]$"))
        {
            read = await ReadAsync(address, length);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(read);
            }
            return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content));
        }
        read = await ReadAsync(address, length);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<bool[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content.Select((m) => m != 0).Take(length).ToArray());
    }

    /// <inheritdoc />
    public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        CommHelper.ExtractParameter(ref address, "type", 0);
        if (length == 1)
        {
            var read = await ReadWithTypeAsync(new string[1] { address }, new ushort[1] { length });
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            return OperateResult.CreateSuccessResult(read.Content1);
        }
        var count = 0;
        var index = 0;
        var format = "[{0}]";
        var array = new List<byte>();
        var match = Regex.Match(address, "\\[[0-9]+\\]$");
        if (match.Success)
        {
            address = address.Remove(match.Index, match.Length);
            index = int.Parse(match.Value.Substring(1, match.Value.Length - 2));
        }
        else
        {
            var match2 = Regex.Match(address, "\\[[0-9]+,[0-9]+\\]$");
            if (match2.Success)
            {
                address = address.Remove(match2.Index, match2.Length);
                var index2 = Regex.Matches(match2.Value, "[0-9]+")[1].Value;
                format = match2.Value.Replace(index2 + "]", "{0}]");
                index = int.Parse(index2);
            }
        }
        ushort dataType = 0;
        while (count < length)
        {
            if (count == 0)
            {
                var first = Math.Min(length, (ushort)248);
                var read = await ReadWithTypeAsync(new string[1] { address + string.Format(format, index) }, new ushort[1] { first });
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(read);
                }
                dataType = read.Content2;
                count += first;
                index += first;
                array.AddRange(read.Content1);
            }
            else
            {
                var len = (ushort)GetLengthFromRemain(dataType, length - count);
                var read = await ReadWithTypeAsync(new string[1] { address + string.Format(format, index) }, new ushort[1] { len });
                if (!read.IsSuccess)
                {
                    return OperateResult.CreateFailedResult<byte[]>(read);
                }
                count += len;
                index += len;
                array.AddRange(read.Content1);
            }
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Read(System.String[],System.UInt16[])" />
    public async Task<OperateResult<byte[]>> ReadAsync(string[] address, ushort[] length)
    {
        var read = await ReadWithTypeAsync(address, length);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content1);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadByte(System.String)" />
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadAsync(address, 1));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.ReadTag(System.String,System.UInt16)" />
    public async Task<OperateResult<ushort, byte[]>> ReadTagAsync(string address, ushort length = 1)
    {
        var read = await ReadWithTypeAsync(new string[1] { address }, new ushort[1] { length });
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<ushort, byte[]>(read);
        }
        return OperateResult.CreateSuccessResult(read.Content2, read.Content1);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.Write(System.String,System.Byte[])" />
    public override OperateResult Write(string address, byte[] value)
    {
        return WriteTag(address, 209, value, !CommHelper.IsAddressEndWithIndex(address) ? 1 : value.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyNet.WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />
    public virtual OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
    {
        typeCode = (ushort)CommHelper.ExtractParameter(ref address, "type", typeCode);
        var operateResult = BuildWriteCommand(address, typeCode, value, length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var operateResult2 = ReadFromCoreServer(operateResult.Content);
        if (!operateResult2.IsSuccess)
        {
            return operateResult2;
        }
        var operateResult3 = AllenBradleyHelper.CheckResponse(operateResult2.Content);
        if (!operateResult3.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(operateResult3);
        }
        return AllenBradleyHelper.ExtractActualData(operateResult2.Content, isRead: false);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Byte[])" />
    public override async Task<OperateResult> WriteAsync(string address, byte[] value)
    {
        return await WriteTagAsync(address, 209, value, !CommHelper.IsAddressEndWithIndex(address) ? 1 : value.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTag(System.String,System.UInt16,System.Byte[],System.Int32)" />
    public virtual async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
    {
        typeCode = (ushort)CommHelper.ExtractParameter(ref address, "type", typeCode);
        var command = BuildWriteCommand(address, typeCode, value, length);
        if (!command.IsSuccess)
        {
            return command;
        }
        var read = await ReadFromCoreServerAsync(command.Content);
        if (!read.IsSuccess)
        {
            return read;
        }
        var check = AllenBradleyHelper.CheckResponse(read.Content);
        if (!check.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(check);
        }
        return AllenBradleyHelper.ExtractActualData(read.Content, isRead: false);
    }

    /// <inheritdoc />
    public override OperateResult<short[]> ReadInt16(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<ushort[]> ReadUInt16(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransUInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<int[]> ReadInt32(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<uint[]> ReadUInt32(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<float[]> ReadFloat(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransSingle(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<long[]> ReadInt64(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<ulong[]> ReadUInt64(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override OperateResult<double[]> ReadDouble(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(Read(address, length), (m) => ByteTransform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public OperateResult<string> ReadString(string address)
    {
        return ReadString(address, 1, Encoding.UTF8);
    }

    /// <summary>
    /// 读取字符串数据，默认为UTF-8编码<br />
    /// Read string data, default is UTF-8 encoding
    /// </summary>
    /// <param name="address">起始地址</param>
    /// <param name="length">数据长度</param>
    /// <returns>带有成功标识的string数据</returns>
    /// <example>
    /// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
    /// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadString" title="String类型示例" />
    /// </example>
    public override OperateResult<string> ReadString(string address, ushort length)
    {
        return ReadString(address, length, Encoding.UTF8);
    }

    /// <inheritdoc />
    public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
    {
        var operateResult = Read(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        return ExtraStringContent(operateResult.Content, encoding);
    }

    private OperateResult<string> ExtraStringContent(byte[] content, Encoding encoding)
    {
        try
        {
            if (content.Length >= 2)
            {
                int count = ByteTransform.TransUInt16(content, 0);
                return OperateResult.CreateSuccessResult(encoding.GetString(content, 2, count));
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(content));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>("Parse string failed: " + ex.Message + " Source: " + content.ToHexString(' '));
        }
    }

    /// <inheritdoc />
    public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransUInt16(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransSingle(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (m) => ByteTransform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<string>> ReadStringAsync(string address)
    {
        return await ReadStringAsync(address, 1, Encoding.UTF8);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadString(System.String,System.UInt16)" />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await ReadStringAsync(address, length, Encoding.UTF8);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        var read = await ReadAsync(address, length);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        return ExtraStringContent(read.Content, encoding);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, short[] values)
    {
        return WriteTag(address, 195, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, ushort[] values)
    {
        return WriteTag(address, 199, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, int[] values)
    {
        return WriteTag(address, 196, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, uint[] values)
    {
        return WriteTag(address, 200, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, float[] values)
    {
        return WriteTag(address, 202, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, long[] values)
    {
        return WriteTag(address, 197, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteUInt64Array", "")]
    public override OperateResult Write(string address, ulong[] values)
    {
        return WriteTag(address, 201, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteDoubleArray", "")]
    public override OperateResult Write(string address, double[] values)
    {
        return WriteTag(address, 203, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteString", "")]
    public override OperateResult Write(string address, string value)
    {
        return Write(address, value, Encoding.UTF8);
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, string value, Encoding encoding)
    {
        var array = string.IsNullOrEmpty(value) ? new byte[0] : encoding.GetBytes(value);
        return WriteTag(address, 208, SoftBasic.SpliceArray(BitConverter.GetBytes((ushort)array.Length), array));
    }

    /// <inheritdoc />
    [HslMqttApi("WriteBool", "")]
    public override OperateResult Write(string address, bool value)
    {
        return WriteTag(address, 193, !value ? new byte[2] : new byte[2] { 255, 255 });
    }

    /// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean[])" />
    [HslMqttApi("WriteBoolArray", "")]
    public override OperateResult Write(string address, bool[] value)
    {
        return WriteTag(address, 193, value.Select((m) => (byte)(m ? 1 : 0)).ToArray(), !CommHelper.IsAddressEndWithIndex(address) ? 1 : value.Length);
    }

    /// <inheritdoc />
    [HslMqttApi("WriteByte", "")]
    public OperateResult Write(string address, byte value)
    {
        return WriteTag(address, 194, new byte[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Int16[])" />
    public override async Task<OperateResult> WriteAsync(string address, short[] values)
    {
        return await WriteTagAsync(address, 195, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.UInt16[])" />
    public override async Task<OperateResult> WriteAsync(string address, ushort[] values)
    {
        return await WriteTagAsync(address, 199, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Int32[])" />
    public override async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteTagAsync(address, 196, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.UInt32[])" />
    public override async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteTagAsync(address, 200, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Single[])" />
    public override async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteTagAsync(address, 202, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Int64[])" />
    public override async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteTagAsync(address, 197, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.UInt64[])" />
    public override async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteTagAsync(address, 201, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Double[])" />
    public override async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteTagAsync(address, 203, ByteTransform.TransByte(values), values.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.String)" />
    public override async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await WriteAsync(address, value, Encoding.UTF8);
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        var buffer = string.IsNullOrEmpty(value) ? new byte[0] : encoding.GetBytes(value);
        return await WriteTagAsync(address, 208, SoftBasic.SpliceArray(BitConverter.GetBytes((ushort)buffer.Length), buffer));
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Boolean)" />
    public override async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await WriteTagAsync(address, 193, !value ? new byte[2] : new byte[2] { 255, 255 });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Boolean[])" />
    public override async Task<OperateResult> WriteAsync(string address, bool[] value)
    {
        return await WriteTagAsync(address, 193, value.Select((m) => (byte)(m ? 1 : 0)).ToArray(), !CommHelper.IsAddressEndWithIndex(address) ? 1 : value.Length);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.Write(System.String,System.Byte)" />
    public async Task<OperateResult> WriteAsync(string address, byte value)
    {
        return await WriteTagAsync(address, 194, new byte[1] { value });
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String)" />
    public OperateResult<DateTime> ReadDate(string address)
    {
        return AllenBradleyHelper.ReadDate(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.DateTime)" />
    public OperateResult WriteDate(string address, DateTime date)
    {
        return AllenBradleyHelper.WriteDate(this, address, date);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteDate(System.String,System.DateTime)" />
    public OperateResult WriteTimeAndDate(string address, DateTime date)
    {
        return AllenBradleyHelper.WriteTimeAndDate(this, address, date);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.ReadTime(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String)" />
    public OperateResult<TimeSpan> ReadTime(string address)
    {
        return AllenBradleyHelper.ReadTime(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteTime(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.TimeSpan)" />
    public OperateResult WriteTime(string address, TimeSpan time)
    {
        return AllenBradleyHelper.WriteTime(this, address, time);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.AllenBradley.AllenBradleyHelper.WriteTimeOfDate(HslCommunication.Profinet.AllenBradley.IReadWriteCip,System.String,System.TimeSpan)" />
    public OperateResult WriteTimeOfDate(string address, TimeSpan timeOfDate)
    {
        return AllenBradleyHelper.WriteTimeOfDate(this, address, timeOfDate);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadDate(System.String)" />
    public async Task<OperateResult<DateTime>> ReadDateAsync(string address)
    {
        return await AllenBradleyHelper.ReadDateAsync(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteDate(System.String,System.DateTime)" />
    public async Task<OperateResult> WriteDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteDateAsync(this, address, date);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTimeAndDate(System.String,System.DateTime)" />
    public async Task<OperateResult> WriteTimeAndDateAsync(string address, DateTime date)
    {
        return await AllenBradleyHelper.WriteTimeAndDateAsync(this, address, date);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.ReadTime(System.String)" />
    public async Task<OperateResult<TimeSpan>> ReadTimeAsync(string address)
    {
        return await AllenBradleyHelper.ReadTimeAsync(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTime(System.String,System.TimeSpan)" />
    public async Task<OperateResult> WriteTimeAsync(string address, TimeSpan time)
    {
        return await AllenBradleyHelper.WriteTimeAsync(this, address, time);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Omron.OmronConnectedCipNet.WriteTimeOfDate(System.String,System.TimeSpan)" />
    public async Task<OperateResult> WriteTimeOfDateAsync(string address, TimeSpan timeOfDate)
    {
        return await AllenBradleyHelper.WriteTimeOfDateAsync(this, address, timeOfDate);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"OmronConnectedCipNet[{IpAddress}:{Port}]";
    }
}
