using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// HostLinkCMode协议的接口信息
/// </summary>
public interface IHostLinkCMode : IReadWriteNet
{
    /// <summary>
    /// 读取PLC的当前的型号信息。
    /// </summary>
    /// <returns>型号</returns>
    Task<OperateResult<string>> ReadPlcTypeAsync();

    /// <summary>
    /// 读取PLC的当前的型号信息。
    /// </summary>
    /// <param name="unitNumber">站号信息</param>
    /// <returns>型号</returns>
    Task<OperateResult<string>> ReadPlcTypeAsync(byte unitNumber);

    /// <summary>
    ///  读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <returns>0: 编程模式  1: 运行模式  2: 监视模式</returns>
    Task<OperateResult<int>> ReadPlcModeAsync();

    /// <summary>
    ///  读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <param name="unitNumber">站号信息</param>
    /// <returns>0: 编程模式  1: 运行模式  2: 监视模式</returns>
    Task<OperateResult<int>> ReadPlcModeAsync(byte unitNumber);

    /// <summary>
    /// 将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <param name="mode">0: 编程模式  1: 运行模式  2: 监视模式</param>
    /// <returns>是否变更成功</returns>
    Task<OperateResult> ChangePlcMode(byte mode);

    /// <summary>
    /// 将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <param name="unitNumber">站号信息</param>
    /// <param name="mode">0: 编程模式  1: 运行模式  2: 监视模式</param>
    /// <returns>是否变更成功</returns>
    Task<OperateResult> ChangePlcModeAsync(byte unitNumber, byte mode);
}
