namespace ThingsEdge.Server.Pages.Devices;

public sealed class OeeChatViewModel
{
    public object BuildOeeChatOption(IEnumerable<OEEDto> source)
    {
        Random rand = new();
        OEEDto[] source2 = new OEEDto[26];
        for (var i = 0; i <= 25; i++)
        {
            source2[i] = new()
            {
                EquipmentCode = $"OP{(i + 1) * 10:D3}",
                LoadingTime = Math.Round(rand.Next(1500, 2000) * 1.1, 2),
                WarningTime = Math.Round(rand.Next(100, 200) * 1.1, 2),
                EStopingTime = Math.Round(rand.Next(100, 200) * 1.1, 2),
                WorkingTime = Math.Round(rand.Next(100, 200) * 1.1, 2),
                PerformanceRate = Math.Round(rand.NextDouble(), 2),
                OkCount = rand.Next(100, 200),
                NgCount = rand.Next(0, 100),
                TotalCount = rand.Next(100, 300),
                YieldRate = Math.Round(rand.NextDouble(), 2),
            };
        }

        return new
        {
            Title = new
            {
                Left = "center",
                Text = "设备性能效率"
            },
            Tooltip = new
            {
                Formatter = new StringBuilder("function(params) { ")
                                .Append("if (params.componentType === 'markLine') return `${params.name}: ${params.value}`; ")
                                .Append("return `")
                                .Append("${params.name}<br/>")
                                .Append("负荷时间: ${params.value.loadingTime}<br/>")
                                .Append("急停时间: ${params.value.eStopingTime}<br/>")
                                .Append("加工时间: ${params.value.workingTime}<br/>")
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
                Source = source2,
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
