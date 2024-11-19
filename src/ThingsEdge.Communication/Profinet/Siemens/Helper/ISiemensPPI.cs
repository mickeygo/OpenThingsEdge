using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Siemens.Helper;

/// <summary>
/// 西门子PPI的公用接口信息
/// </summary>
public interface ISiemensPPI : IReadWriteNet
{
    /// <summary>
    /// 启动西门子PLC为RUN模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。
    /// </summary>
    /// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
    /// <returns>是否启动成功</returns>
    Task<OperateResult> StartAsync(string parameter = "");

    /// <summary>
    /// 停止西门子PLC，切换为Stop模式，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。
    /// </summary>
    /// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
    /// <returns>是否停止成功</returns>
    Task<OperateResult> StopAsync(string parameter = "");

    /// <summary>
    /// 读取西门子PLC的型号信息，参数信息可以携带站号信息 "s=2;", 注意，分号是必须的。
    /// </summary>
    /// <param name="parameter">额外的参数信息，例如可以携带站号信息 "s=2;", 注意，分号是必须的。</param>
    /// <returns>包含是否成功的结果对象</returns>
    Task<OperateResult<string>> ReadPlcTypeAsync(string parameter = "");
}
