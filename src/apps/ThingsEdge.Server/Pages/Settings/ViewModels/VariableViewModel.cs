using ThingsEdge.Contracts.Variables;

namespace ThingsEdge.Server.Pages.Settings;

/// <summary>
/// 变量 ViewModel
/// </summary>
internal sealed class VariableViewModel
{
    /// <summary>
    /// 驱动类型
    /// </summary>
    public IEnumerable<DriverModel> DriverModels { get; set; } = Enumerable.Empty<DriverModel>();

    public IEnumerable<TagDataType> TagDataTypes { get; set; } = Enumerable.Empty<TagDataType>();

    public IEnumerable<TagFlag> TagFlags { get; set; } = Enumerable.Empty<TagFlag>();

    public IEnumerable<TagUsage> TagUsages { get; set; } = Enumerable.Empty<TagUsage>();
}
