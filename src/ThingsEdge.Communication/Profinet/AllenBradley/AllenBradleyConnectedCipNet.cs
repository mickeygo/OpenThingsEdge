using ThingsEdge.Communication.BasicFramework;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.Profinet.Omron;

namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// 基于连接的对象访问的CIP协议的实现，用于对罗克韦尔 PLC进行标签的数据读写，对数组，多维数组进行读写操作，支持的数据类型请参照API文档手册。<br />
/// The implementation of the CIP protocol based on connected object access is used to read and write tag data to Rockwell PLC, 
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
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage" title="实例化及连接示例" />
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
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage2" title="读写示例" />
/// 对于 CESHI_B 来说，写入的操作有点特殊
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage3" title="读写示例" />
/// 对于 CESHI_C, CESHI_D 来说，就是 ReadInt16(string address) , Write( string address, short value ) 和 ReadUInt16(string address) 和 Write( string address, ushort value ) 差别不大。
/// 所以我们着重来看看数组的情况，以 CESHI_G 标签为例子:<br />
/// 情况一，我想一次读取这个标签所有的字节数组（当长度满足的情况下，会一次性返回数据）
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage4" title="读写示例" />
/// 情况二，我想读取第3个数，或是第6个数开始，一共5个数
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage5" title="读写示例" />
/// 其他的数组情况都是类似的，我们来看看字符串 CESHI_J 变量
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage6" title="读写示例" />
/// 对于 bool 变量来说，就是 ReadBool("CESHI_O") 和 Write("CESHI_O", true) 操作，如果是bool数组，就不一样了
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage7" title="读写示例" />
/// 最后我们来看看结构体的操作，假设我们有个结构体<br />
/// MyData.Code     STRING(12)<br />
/// MyData.Value1   INT<br />
/// MyData.Value2   INT<br />
/// MyData.Value3   REAL<br />
/// MyData.Value4   INT<br />
/// MyData.Value5   INT<br />
/// MyData.Value6   INT[0..3]<br />
/// 因为bool比较复杂，暂时不考虑。要读取上述的结构体，我们需要定义结构一样的数据
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage8" title="结构体" />
/// 定义好后，我们再来读取就很简单了。
/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\AllenBradleyConnectedCipNetSample.cs" region="Usage9" title="读写示例" />
/// </example>
public class AllenBradleyConnectedCipNet : OmronConnectedCipNet
{
    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public AllenBradleyConnectedCipNet()
    {
    }

    /// <summary>
    /// 根据指定的IP及端口来实例化这个连接对象
    /// </summary>
    /// <param name="ipAddress">PLC的Ip地址</param>
    /// <param name="port">PLC的端口号信息</param>
    public AllenBradleyConnectedCipNet(string ipAddress, int port = 44818)
        : base(ipAddress, port)
    {
    }

    /// <inheritdoc />
    protected override int GetMaxTransferBytes()
    {
        return 1980;
    }

    /// <inheritdoc />
    public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
    {
        var operateResult = Read(address, length);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        try
        {
            if (operateResult.Content.Length >= 6)
            {
                var count = ByteTransform.TransInt32(operateResult.Content, 2);
                return OperateResult.CreateSuccessResult(encoding.GetString(operateResult.Content, 6, count));
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(operateResult.Content));
        }
        catch (Exception ex)
        {
            return new OperateResult<string>(ex.Message + " Source: " + operateResult.Content.ToHexString(' '));
        }
    }

    /// <inheritdoc />
    public override OperateResult Write(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var bytes = encoding.GetBytes(value);
        var operateResult = Write(address + ".LEN", bytes.Length);
        if (!operateResult.IsSuccess)
        {
            return operateResult;
        }
        var value2 = SoftBasic.ArrayExpandToLengthEven(bytes);
        return WriteTag(address + ".DATA[0]", 194, value2, bytes.Length);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        var read = await ReadAsync(address, length);
        if (!read.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(read);
        }
        if (read.Content.Length >= 6)
        {
            return OperateResult.CreateSuccessResult(encoding.GetString(count: ByteTransform.TransInt32(read.Content, 2), bytes: read.Content, index: 6));
        }
        return OperateResult.CreateSuccessResult(encoding.GetString(read.Content));
    }

    /// <inheritdoc />
    public override async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
        }
        var data = encoding.GetBytes(value);
        var write = await WriteAsync(address + ".LEN", data.Length);
        if (!write.IsSuccess)
        {
            return write;
        }
        return await WriteTagAsync(value: SoftBasic.ArrayExpandToLengthEven(data), address: address + ".DATA[0]", typeCode: 194, length: data.Length);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"AllenBradleyConnectedCipNet[{IpAddress}:{Port}]";
    }
}
