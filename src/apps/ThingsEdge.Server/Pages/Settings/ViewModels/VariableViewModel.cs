namespace ThingsEdge.Server.Pages.Settings;

/// <summary>
/// 变量 ViewModel
/// </summary>
internal sealed class VariableViewModel : ITransientDependency
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceTagDataSnapshot _deviceTagDataSnapshot;

    public VariableViewModel(IDeviceService deviceService, IDeviceTagDataSnapshot deviceTagDataSnapshot)
    {
        _deviceService = deviceService;
        _deviceTagDataSnapshot = deviceTagDataSnapshot;
    }

    public static IEnumerable<string> DriverModels { get; set; } = EnumUtil.FetchStrings<DriverModel>(true);

    public static IEnumerable<string> TagDataTypes { get; set; } = EnumUtil.FetchStrings<TagDataType>(true);

    public static IEnumerable<string> TagFlags { get; set; } = EnumUtil.FetchStrings<TagFlag>(true);

    public static IEnumerable<string> TagUsages { get; set; } = EnumUtil.FetchStrings<TagUsage>(true);

    public static List<DataTableHeader<TagModel>> GetHeaders(string? category) 
    {
        if (category is DeviceTreeCategory.Device or DeviceTreeCategory.TagGroup)
        {
            return new()
            {
              new (){ Text = "标记", Value = nameof(TagModel.Name), Width = 200 },
              new (){ Text = "地址", Value = nameof(TagModel.Address), Width = 80 },
              new (){ Text = "长度", Value = nameof(TagModel.Length), Width = 80 },
              new (){ Text = "数据类型", Value = nameof(TagModel.DataType), Width = 100 },
              new (){ Text = "扫描速率(ms)", Value = nameof(TagModel.ScanRate), Width = 120 },
              new (){ Text = "标识", Value = nameof(TagModel.Flag), Width = 80 },
              new (){ Text = "用途", Value = nameof(TagModel.Usage), Width = 100 },
              new (){ Text = "发布模式", Value = nameof(TagModel.PublishMode), Width = 100 },
              new (){ Text = "要旨", Value = nameof(TagModel.Keynote), Width = 100 },
              new (){ Text = "描述", Value = nameof(TagModel.Description), Width = 120 },
              new (){ Text = "文本值", Value = nameof(TagModel.Text), Width = 200 },
            };
        }

        return new List<DataTableHeader<TagModel>>(0);
    }

    public List<TreeModel> GetDeviceTree() => _deviceService.GetDeviceTree();

    public List<TagModel>? GetTags(string id, string? category) => _deviceService.GetTags(id, category);

    public string? GetTagDataSnapshot(string tagId) => _deviceTagDataSnapshot.Get(tagId)?.GetString();

    public TreeModel? FindTreeItem(List<TreeModel>? treeItems, string id)
    {
        if (treeItems is null || !treeItems.Any())
        {
            return default;
        }

        foreach (var treeItem in treeItems)
        {
            if (treeItem.Id == id)
            {
                return treeItem;
            }

            var item = FindTreeItem(treeItem.Children, id);
            if (item is not null)
            {
                return item;
            }
        }

        return default;
    }
}
