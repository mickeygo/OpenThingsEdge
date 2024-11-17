using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Delta;

/// <summary>
/// 台达PLC的相关的接口信息
/// </summary>
public interface IDelta : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 获取或设置当前的台达PLC的系列信息，默认为 DVP 系列<br />
    /// Get or set the current series information of Delta PLC, the default is DVP series
    /// </summary>
    DeltaSeries Series { get; set; }
}
