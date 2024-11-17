namespace ThingsEdge.Communication.Profinet.AllenBradley;

/// <summary>
/// 结构体的句柄信息
/// </summary>
public class AbStructHandle
{
    /// <summary>
    /// 返回项数
    /// </summary>
    /// <remarks>
    /// Count of Items returned
    /// </remarks>
    public ushort ReturnCount { get; set; }

    /// <summary>
    /// 结构体定义大小
    /// </summary>
    /// <remarks>
    /// This is the number of structure members
    /// </remarks>
    public uint TemplateObjectDefinitionSize { get; set; }

    /// <summary>
    /// 使用读取标记服务读取结构时在线路上传输的字节数
    /// </summary>
    /// <remarks>
    /// This is the number of bytes of the structure data
    /// </remarks>
    public uint TemplateStructureSize { get; set; }

    /// <summary>
    /// 成员数量
    /// </summary>
    /// <remarks>
    /// This is the number of structure members
    /// </remarks>
    public ushort MemberCount { get; set; }

    /// <summary>
    /// 结构体的handle
    /// </summary>
    /// <remarks>
    /// This is the Tag Type Parameter used in Read/Write Tag service
    /// </remarks>
    public ushort StructureHandle { get; set; }

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// instantiate a default object
    /// </summary>
    public AbStructHandle()
    {
    }

    /// <summary>
    /// 使用原始字节的数据，索引信息来实例化一个对象<br />
    /// Instantiate an object with raw bytes of data, index information
    /// </summary>
    /// <param name="source">原始字节数据</param>
    /// <param name="index">起始的偏移索引</param>
    public AbStructHandle(byte[] source, int index)
    {
        ReturnCount = BitConverter.ToUInt16(source, index);
        TemplateObjectDefinitionSize = BitConverter.ToUInt32(source, index + 6);
        TemplateStructureSize = BitConverter.ToUInt32(source, index + 14);
        MemberCount = BitConverter.ToUInt16(source, index + 22);
        StructureHandle = BitConverter.ToUInt16(source, index + 28);
    }
}
