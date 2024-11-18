using ThingsEdge.Communication.ModBus;

namespace ThingsEdge.Communication.Profinet.XINJE;

/// <summary>
/// 信捷PLC的XC,XD,XL系列的串口转网口通讯类，虽然硬件层走的是TCP协议，但是底层使用ModbusRtu协议实现，每个系列支持的地址类型及范围不一样。
/// </summary>
/// <remarks>
/// 对于XC系列适用于XC1/XC2/XC3/XC5/XCM/XCC系列，线圈支持X,Y,S,M,T,C，寄存器支持D,F,E,T,C<br />
/// 对于XD,XL系列适用于XD1/XD2/XD3/XD5/XDM/XDC/XD5E/XDME/XDH/XL1/XL3/XL5/XL5E/XLME，
/// 线圈支持X,Y,S,M,SM,T,C,ET,SEM,HM,HS,HT,HC,HSC 寄存器支持D,ID,QD,SD,TD,CD,ETD,HD,HSD,HTD,HCD,HSCD,FD,SFD,FS<br />
/// </remarks>
public class XinJESerialOverTcp : ModbusRtuOverTcp
{
    /// <summary>
    /// 获取或设置当前的信捷PLC的系列，默认XC系列
    /// </summary>
    public XinJESeries Series { get; set; }

    /// <summary>
    /// 实例化一个默认的对象
    /// </summary>
    public XinJESerialOverTcp()
    {
        Series = XinJESeries.XC;
    }

    /// <summary>
    /// 通过指定站号，ip地址，端口号来实例化一个新的对象
    /// </summary>
    /// <param name="ipAddress">Ip地址</param>
    /// <param name="port">端口号</param>
    /// <param name="station">站号信息</param>
    public XinJESerialOverTcp(string ipAddress, int port = 502, byte station = 1)
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
    public XinJESerialOverTcp(XinJESeries series, string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
        Series = series;
    }

    /// <inheritdoc />
    public override OperateResult<string> TranslateToModbusAddress(string address, byte modbusCode)
    {
        return XinJEHelper.PraseXinJEAddress(Series, address, modbusCode);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"XinJESerialOverTcp<{Series}>[{IpAddress}:{Port}]";
    }
}
