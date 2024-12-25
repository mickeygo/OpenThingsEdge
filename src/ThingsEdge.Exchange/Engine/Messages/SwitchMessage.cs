using ThingsEdge.Exchange.Contracts.Variables;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Infrastructure.Brokers;

namespace ThingsEdge.Exchange.Engine.Messages;

/// <summary>
/// 开关消息。
/// </summary>
/// <param name="Connector">连接驱动</param>
/// <param name="ChannelName">通道名称</param>
/// <param name="Device">设备信息</param>
/// <param name="Tag">标记</param>
/// <param name="Self">读取的信号点标记值</param>
/// <param name="State">开关状态</param>
/// <param name="IsSwitchSignal">是否为开关信号。注：开关标识，只表示数据为开关信号，否则为开关下具体的数据。</param>
internal sealed record SwitchMessage(IDriverConnector Connector,
    string ChannelName,
    Device Device,
    Tag Tag,
    PayloadData? Self,
    SwitchState State = SwitchState.None,
    bool IsSwitchSignal = false) : IMessage;

/// <summary>
/// 开关状态。
/// </summary>
public enum SwitchState
{
    /// <summary>
    /// 未知
    /// </summary>
    None,

    /// <summary>
    /// 开启
    /// </summary>
    On,

    /// <summary>
    /// 关闭
    /// </summary>
    Off,
}
