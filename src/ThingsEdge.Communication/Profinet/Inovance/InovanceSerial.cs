using System.Text.RegularExpressions;
using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.HslCommunication;
using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.Inovance;

/// <summary>
/// 汇川的串口通信协议，A适用于AM400、 AM400_800、 AC800、H3U, XP, H5U 等系列底层走的是MODBUS-RTU协议，地址说明参见标记<br />
/// Huichuan's serial communication protocol is applicable to AM400, AM400_800, AC800 and other series. The bottom layer is MODBUS-RTU protocol. For the address description, please refer to the mark
/// </summary>
/// <remarks>
/// AM400_800 的元件有 Q 区，I 区，M 区这三种，分别都可以按位，按字节，按字和按双字进行访问，在本组件的条件下，仅支持按照位，字访问。<br />
/// 位地址支持 Q, I, M 地址类型，字地址支持 SM, SD，支持对字地址的位访问，例如 ReadBool("SD0.5");
/// H3U 系列控制器支持 M/SM/S/T/C/X/Y 等 bit 型变量（也称线圈） 的访问、 D/SD/R/T/C 等 word 型变量的访问；<br />
/// H5U 系列控制器支持 M/B/S/X/Y 等 bit 型变量（也称线圈） 的访问、 D/R 等 word 型变量的访问；内部 W 元件，不支持通信访问。<br />
/// </remarks>
public class InovanceSerial : ModbusRtu
{
    /// <summary>
    /// 获取或设置汇川的系列，默认为AM系列
    /// </summary>
    public InovanceSeries Series { get; set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public InovanceSerial()
    {
        Series = InovanceSeries.AM;
        DataFormat = DataFormat.CDAB;
    }

    /// <summary>
    /// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
    /// Specify the server address, port number, and client's own station number to initialize
    /// </summary>
    /// <param name="station">客户端自身的站号</param>
    public InovanceSerial(byte station = 1)
        : base(station)
    {
        Series = InovanceSeries.AM;
        DataFormat = DataFormat.CDAB;
    }

    /// <summary>
    /// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
    /// Specify the server address, port number, and client's own station number to initialize
    /// </summary>
    /// <param name="series">PLC的系列选择</param>
    /// <param name="station">客户端自身的站号</param>
    public InovanceSerial(InovanceSeries series, byte station = 1)
        : base(station)
    {
        Series = series;
        Series = InovanceSeries.AM;
        DataFormat = DataFormat.CDAB;
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.ReadByte(HslCommunication.ModBus.IModbus,System.String)" />
    [HslMqttApi("ReadByte", "")]
    public OperateResult<byte> ReadByte(string address)
    {
        return InovanceHelper.ReadByte(this, address);
    }

    /// <inheritdoc cref="M:HslCommunication.Profinet.Inovance.InovanceHelper.ReadByte(HslCommunication.ModBus.IModbus,System.String)" />
    public async Task<OperateResult<byte>> ReadByteAsync(string address)
    {
        return await InovanceHelper.ReadByteAsync(this, address);
    }

    /// <inheritdoc />
    public override OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        return InovanceHelper.PraseInovanceAddress(Series, address, modbusCode);
    }

    /// <inheritdoc />
    public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
    {
        if (Series == InovanceSeries.AM && Regex.IsMatch(address, "MB[0-9]*[13579]$", RegexOptions.IgnoreCase))
        {
            return InovanceHelper.ReadAMString(this, address, length, encoding);
        }
        return base.ReadString(address, length, encoding);
    }

    /// <inheritdoc />
    public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        if (Series == InovanceSeries.AM && Regex.IsMatch(address, "MB[0-9]*[13579]$", RegexOptions.IgnoreCase))
        {
            return await InovanceHelper.ReadAMStringAsync(this, address, length, encoding);
        }
        return await base.ReadStringAsync(address, length, encoding);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"InovanceSerial<{Series}>[{PortName}:{BaudRate}]";
    }
}
