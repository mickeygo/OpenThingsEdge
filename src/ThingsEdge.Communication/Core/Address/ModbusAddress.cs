namespace ThingsEdge.Communication.Core.Address;

/// <summary>
/// Modbus协议地址格式，可以携带站号，功能码，地址信息<br />
/// Modbus protocol address format, can carry station number, function code, address information
/// </summary>
public class ModbusAddress : DeviceAddressDataBase
{
    /// <summary>
    /// 获取或设置当前地址的站号信息<br />
    /// Get or set the station number information of the current address
    /// </summary>
    public int Station { get; set; }

    /// <summary>
    /// 获取或设置当前地址携带的功能码<br />
    /// Get or set the function code carried by the current address
    /// </summary>
    public int Function { get; set; }

    /// <summary>
    /// 获取或设置当前地址在写入的情况下使用的功能码，用来扩展一些非常特殊的自定义服务器<br />
    /// </summary>
    public int WriteFunction { get; set; }

    /// <summary>
    /// 实例化一个默认的对象<br />
    /// Instantiate a default object
    /// </summary>
    public ModbusAddress()
    {
        Station = -1;
        Function = -1;
        WriteFunction = -1;
        AddressStart = 0;
    }

    /// <summary>
    /// 实例化一个对象，使用指定的地址初始化<br />
    /// Instantiate an object, initialize with the specified address
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
    /// 实例化一个对象，使用指定的地址及功能码初始化<br />
    /// Instantiate an object and initialize it with the specified address and function code
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
    /// 实例化一个对象，使用指定的地址，站号，功能码来初始化<br />
    /// Instantiate an object, use the specified address, station number, function code to initialize
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
        var array = address.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i].StartsWith("s=", StringComparison.OrdinalIgnoreCase))
            {
                Station = byte.Parse(array[i].Substring(2));
            }
            else if (array[i].StartsWith("x=", StringComparison.OrdinalIgnoreCase))
            {
                Function = byte.Parse(array[i].Substring(2));
            }
            else if (array[i].StartsWith("w=", StringComparison.OrdinalIgnoreCase))
            {
                WriteFunction = byte.Parse(array[i].Substring(2));
            }
            else
            {
                AddressStart = ushort.Parse(array[i]);
            }
        }
    }

    /// <summary>
    /// 地址偏移指定的位置，返回一个新的地址对象<br />
    /// The address is offset by the specified position and a new address object is returned
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
    /// 地址偏移1，返回一个新的地址对象<br />
    /// The address is offset by 1 and a new address object is returned
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
            stringBuilder.Append("s=" + Station + ";");
        }
        if (Function == 2 || Function == 4 || Function > 6)
        {
            stringBuilder.Append("x=" + Function + ";");
        }
        if (WriteFunction > 0)
        {
            stringBuilder.Append("w=" + WriteFunction + ";");
        }
        stringBuilder.Append(AddressStart.ToString());
        return stringBuilder.ToString();
    }
}
