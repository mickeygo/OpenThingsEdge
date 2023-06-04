namespace ThingsEdge.Server.Pages.Devices;

public sealed class OeeChatViewModel
{
    public IEnumerable<object> BuildOeeChatOptions(IEnumerable<OEEGroupDto> oeeGroups)
    {
        foreach (var oeeGroup in oeeGroups) 
        { 
            yield return BuildOeeChatOption(oeeGroup.Line, oeeGroup.OeeList); 
        }
    }

    public object BuildOeeChatOption(string line, IEnumerable<OEEDto> source)
    {
        return new
        {
            Title = new
            {
                Left = "center",
                Text = $"{line} - 设备性能效率"
            },
            Tooltip = new
            {
                Formatter = new StringBuilder("function(params) { ")
                                .Append("if (params.componentType === 'markLine') return `${params.name}: ${params.value}`; ")
                                .Append("return `")
                                .Append("${params.name}<br/>")
                                .Append("负荷时间: ${params.value.loadingTime}min<br/>")
                                .Append("急停时间: ${params.value.eStopingTime}min<br/>")
                                .Append("加工时间: ${params.value.workingTime}min<br/>")
                                .Append("OK数量: ${params.value.okCount}<br/>")
                                .Append("NG数量: ${params.value.ngCount}<br/>")
                                .Append("总数量: ${params.value.totalCount}<br/>")
                                .Append("良率: ${params.value.yieldRate}<br/>")
                                .Append("`;")
                                .Append(" }")
                                .ToString(),
            },
            Dataset = new
            {
                Dimensions = new[] { "equipmentCode", "performanceRate" },
                Source = source,
            },
            XAxis = new
            {
                Type = "category",
                AxisLabel = new { Rotate = -30 },
            },
            YAxis = new { },
            Series = new[]
            {
                new
                {
                    Type = "bar",
                    Label = new { Show = true, Position = "inside" },
                    MarkLine = new
                    {
                        Data = new[] { new { Type = "average", Name = "平均值" } },
                    },
                }
            },
        };
    }
}
