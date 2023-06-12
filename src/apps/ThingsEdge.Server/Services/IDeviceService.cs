namespace ThingsEdge.Server.Services;

/// <summary>
/// 设备服务。
/// </summary>
public interface IDeviceService
{
    List<TreeModel> GetDeviceTree();

    List<TagModel>? GetTags(string id, string? category);
}
