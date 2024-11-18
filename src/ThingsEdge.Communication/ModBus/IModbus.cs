using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.ModBus;

/// <summary>
/// Modbus设备的接口，用来表示Modbus相关的设备对象，<see cref="ModbusTcpNet" />, <see cref="ModbusRtu" />,
/// <see cref="ModbusAscii" />, <see cref="ModbusRtuOverTcp" /> 均实现了该接口信息。
/// </summary>
public interface IModbus : IReadWriteDevice, IReadWriteNet
{
    /// <inheritdoc cref="ModbusTcpNet.AddressStartWithZero" />
    bool AddressStartWithZero { get; set; }

    /// <inheritdoc cref="ModbusTcpNet.Station" />
    byte Station { get; set; }

    /// <inheritdoc cref="ModbusTcpNet.DataFormat" />
    DataFormat DataFormat { get; set; }

    /// <inheritdoc cref="ModbusTcpNet.IsStringReverse" />
    bool IsStringReverse { get; set; }

    /// <summary>
    /// 获取或是设置当前广播模式对应的站号，广播模式意味着不接收设备方的数据返回操作，默认为 -1，表示不使用广播模式。
    /// </summary>
    int BroadcastStation { get; set; }

    /// <summary>
    /// 获取或设置当前掩码写入的功能码是否激活状态，设置为 false 时，再执行写入位时，会通过读字，修改位，写字的方式来间接实现。
    /// </summary>
    bool EnableWriteMaskCode { get; set; }

    /// <summary>
    /// 将当前的地址信息转换成Modbus格式的地址，如果转换失败，返回失败的消息。默认不进行任何的转换。
    /// </summary>
    /// <param name="address">传入的地址</param>
    /// <param name="modbusCode">Modbus的功能码</param>
    /// <returns>转换之后Modbus的地址</returns>
    OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode);

    /// <summary>
    /// 注册一个新的地址映射关系，注册地址映射关系后，就可以使用新的地址来读写Modbus数据了，通常用于其他的支持Modbus协议的PLC。
    /// </summary>
    /// <param name="mapping">地址映射关系信息</param>
    void RegisteredAddressMapping(Func<string, byte, OperateResult<string>> mapping);

    /// <summary>
    /// 使用0x17功能码来实现同时写入并读取数据的操作，使用一条报文来实现，需要指定读取的地址，长度，写入的地址，写入的数据信息，返回读取的结果数据。
    /// </summary>
    /// <param name="readAddress">读取的地址信息</param>
    /// <param name="length">读取的长度信息</param>
    /// <param name="writeAddress">写入的地址信息</param>
    /// <param name="value">写入的字节数据信息</param>
    /// <returns>读取的结果对象</returns>
    Task<OperateResult<byte[]>> ReadWriteAsync(string readAddress, ushort length, string writeAddress, byte[] value);
}
