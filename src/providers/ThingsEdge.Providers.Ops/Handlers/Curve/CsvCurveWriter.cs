namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 保存为 CSV 文件格式的写入器
/// </summary>
internal sealed class CsvCurveWriter : ICurveWriter
{
    private readonly List<string> _header = new();
    private readonly List<IEnumerable<PayloadData>> _body = new();

    public bool IsClosed { get; private set; }

    public long WrittenCount { get; private set; }

    public string FilePath { get; init; } = null!;

    public CsvCurveWriter(string path)
    {
        FilePath = path;
    }

    public void WriteHeader(IEnumerable<string> header)
    {
        if (IsClosed)
        {
            return;
        }

        _header.AddRange(header);
    }

    public void WriteLineBody(IEnumerable<PayloadData> items)
    {
        if (IsClosed)
        {
            return;
        }

        ++WrittenCount;
        _body.Add(items);
    }

    public async Task SaveAsync()
    {
        if (IsClosed)
        {
            return;
        }

        using StreamWriter sw = new(FilePath);
        await sw.WriteLineAsync(string.Join(",", _header.Select(VaildCsv))).ConfigureAwait(false);
        foreach (var items in _body)
        {
            var first = items.First();

            // body 非数组
            if (!first.IsArray())
            {
                await sw.WriteLineAsync(string.Join(",", items.Select(s => VaildCsv(s.GetString())))).ConfigureAwait(false);
            }
            else
            {
                // body 为数组
                List<string[]> matrix = new(_header.Count);
                foreach (var header in _header)
                {
                    var payload = items.First(s => s.DisplayName == header);
                    matrix.Add(payload.GetStringArray());
                }

                for (int i = 0; i < matrix[0].Length; i++)
                {
                    List<string> data = new(matrix.Count);
                    foreach (var item in matrix)
                    {
                        data.Add(item[i]);
                    }
                    await sw.WriteLineAsync(string.Join(",", data.Select(VaildCsv))).ConfigureAwait(false);
                }
            }
        }
    }

    private static string VaildCsv(string value)
    {
        // 如果字段中有逗号（,），该字段使用双引号（"）括起来；
        // 如果该字段中有双引号，该双引号前要再加一个双引号，然后把该字段使用双引号括起来。
        // abc,d2 => "abc,d2"
        // ab"c,d2 => "ab""c,d2"
        // "abc => """abc"
        // "" => """"""

        // 目前只做含有逗号的处理
        if (value.Contains(','))
        {
            return $"\"{value}\"";
        }

        return value;
    }

    public void Close()
    {
        if (!IsClosed)
        {
            IsClosed = true;
        }
    }
}
