namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// Modbus协议地址格式，可以携带站号，功能码，地址信息。
/// </summary>
public class ModbusAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置当前地址的站号信息。
    /// </summary>
    public int Station { get; set; }

    /// <summary>
    /// 获取或设置当前地址携带的功能码。
    /// </summary>
    public int Function { get; set; }

    /// <summary>
    /// 获取或设置当前地址在写入的情况下使用的功能码，用来扩展一些非常特殊的自定义服务器。
    /// </summary>
    public int WriteFunction { get; set; }

    /// <summary>
    /// 实例化一个默认的对象。
    /// </summary>
    public ModbusAddress()
    {
        Station = -1;
        Function = -1;
        WriteFunction = -1;
        AddressStart = 0;
    }

    /// <summary>
    /// 实例化一个对象，使用指定的地址初始化。
    /// </summary>
    /// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
    public ModbusAddress(string address)
    {
        Station = -1;
        Function = -1;
        WriteFunction = -1;
        AddressStart = 0;
        Parse(address, 1);
    }

    /// <summary>
    /// 实例化一个对象，使用指定的地址及功能码初始化。
    /// </summary>
    /// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
    /// <param name="function">默认的功能码信息</param>
    public ModbusAddress(string address, byte function)
    {
        Station = -1;
        WriteFunction = -1;
        Function = function;
        AddressStart = 0;
        Parse(address, 1);
    }

    /// <summary>
    /// 实例化一个对象，使用指定的地址，站号，功能码来初始化。
    /// </summary>
    /// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
    /// <param name="station">站号信息</param>
    /// <param name="function">默认的功能码信息</param>
    public ModbusAddress(string address, byte station, byte function)
    {
        WriteFunction = -1;
        Function = function;
        Station = station;
        AddressStart = 0;
        Parse(address, 1);
    }

    /// <inheritdoc />
    public override void Parse(string address, ushort length)
    {
        Length = length;
        if (address.IndexOf(';') < 0)
        {
            AddressStart = ushort.Parse(address);
            return;
        }

        var array = address.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i].StartsWith("s=", StringComparison.OrdinalIgnoreCase))
            {
                Station = byte.Parse(array[i][2..]);
            }
            else if (array[i].StartsWith("x=", StringComparison.OrdinalIgnoreCase))
            {
                Function = byte.Parse(array[i][2..]);
            }
            else if (array[i].StartsWith("w=", StringComparison.OrdinalIgnoreCase))
            {
                WriteFunction = byte.Parse(array[i][2..]);
            }
            else
            {
                AddressStart = ushort.Parse(array[i]);
            }
        }
    }

    /// <summary>
    /// 地址偏移指定的位置，返回一个新的地址对象。
    /// </summary>
    /// <param name="value">数据值信息</param>
    /// <returns>新增后的地址信息</returns>
    public ModbusAddress AddressAdd(int value)
    {
        return new ModbusAddress
        {
            Station = Station,
            Function = Function,
            WriteFunction = WriteFunction,
            AddressStart = AddressStart + value
        };
    }

    /// <summary>
    /// 地址偏移1，返回一个新的地址对象。
    /// </summary>
    /// <returns>新增后的地址信息</returns>
    public ModbusAddress AddressAdd()
    {
        return AddressAdd(1);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        if (Station >= 0)
        {
            stringBuilder.AppendFormat("s={0};", Station);
        }
        if (Function == 2 || Function == 4 || Function > 6)
        {
            stringBuilder.AppendFormat("x={0};", Function);
        }
        if (WriteFunction > 0)
        {
            stringBuilder.AppendFormat("w={0};", WriteFunction);
        }
        stringBuilder.Append(AddressStart);
        return stringBuilder.ToString();
    }
}
