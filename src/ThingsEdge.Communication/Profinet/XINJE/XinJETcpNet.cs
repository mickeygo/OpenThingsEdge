using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.XINJE;

/// <summary>
/// 信捷PLC的XC,XD,XL系列的网口通讯类，底层使用ModbusTcp协议实现，每个系列支持的地址类型及范围不一样。
/// </summary>
/// <remarks>
/// 对于XC系列适用于XC1/XC2/XC3/XC5/XCM/XCC系列，线圈支持X,Y,S,M,T,C，寄存器支持D,F,E,T,C，
/// 对于XD,XL系列适用于XD1/XD2/XD3/XD5/XDM/XDC/XD5E/XDME/XDH/XL1/XL3/XL5/XL5E/XLME，
/// 线圈支持X,Y,S,M,SM,T,C,ET,SEM,HM,HS,HT,HC,HSC 寄存器支持D,ID,QD,SD,TD,CD,ETD,HD,HSD,HTD,HCD,HSCD,FD,SFD,FS。
/// </remarks>
public class XinJETcpNet : ModbusTcpNet
{
    /// <summary>
    /// 获取或设置当前的信捷PLC的系列，默认XC系列。
    /// </summary>
    public XinJESeries Series { get; set; }

    /// <summary>
    /// 通过指定站号，ip地址，端口号来实例化一个新的对象。
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    /// <param name="station">站号信息</param>
    public XinJETcpNet(string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
        Series = XinJESeries.XC;
    }

    /// <summary>
    /// 通过指定站号，IP地址，端口以及PLC的系列来实例化一个新的对象。
    /// </summary>
    /// <param name="series">PLC的系列</param>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    /// <param name="station">站号信息</param>
    public XinJETcpNet(XinJESeries series, string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
        Series = series;
    }

    public override OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        return XinJEHelper.PraseXinJEAddress(Series, address, modbusCode);
    }

    public override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
    {
        return XinJEHelper.ReadAsync(this, address, length, base.ReadAsync);
    }

    public override Task<OperateResult> WriteAsync(string address, byte[] data)
    {
        return XinJEHelper.WriteAsync(this, address, data, base.WriteAsync);
    }

    public override Task<OperateResult> WriteAsync(string address, short value)
    {
        return XinJEHelper.WriteAsync(this, address, value, base.WriteAsync);
    }

    public override Task<OperateResult> WriteAsync(string address, ushort value)
    {
        return XinJEHelper.WriteAsync(this, address, value, base.WriteAsync);
    }

    public override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
    {
        return XinJEHelper.ReadBoolAsync(this, address, length, base.ReadBoolAsync);
    }

    public override Task<OperateResult> WriteAsync(string address, bool[] values)
    {
        return XinJEHelper.WriteAsync(this, address, values, base.WriteAsync);
    }

    public override Task<OperateResult> WriteAsync(string address, bool value)
    {
        return XinJEHelper.WriteAsync(this, address, value, base.WriteAsync);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"XinJETcpNet<{Series}>[{IpAddress}:{Port}]";
    }
}
