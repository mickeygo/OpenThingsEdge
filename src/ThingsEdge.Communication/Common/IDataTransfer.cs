namespace ThingsEdge.Communication.Common;

/// <summary>
/// 用于PLC通讯及ModBus自定义数据类型的读写操作
/// </summary>
/// <remarks>
/// 主要应用于设备实现设备类的自定义的数据类型读写，以此达到简化代码的操作，但是有一个前提，该数据处于连续的数据区块
/// </remarks>
public interface IDataTransfer
{
    /// <summary>
    /// 读取的数据长度，对于西门子，等同于字节数，对于三菱和Modbus为字节数的一半
    /// </summary>
    ushort ReadCount { get; }

    /// <summary>
    /// 从字节数组进行解析实际的对象
    /// </summary>
    /// <param name="Content">从远程读取的数据源</param>
    void ParseSource(byte[] Content);

    /// <summary>
    /// 将对象生成字符源，写入PLC中
    /// </summary>
    /// <returns>准备写入到远程的数据</returns>
    byte[] ToSource();
}
