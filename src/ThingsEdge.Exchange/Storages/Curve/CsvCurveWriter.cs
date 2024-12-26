using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线保存为 CSV 文件格式的写入器。
/// </summary>
internal sealed class CsvCurveWriter(string path, string relativePath) : ICurveWriter
{
    private readonly List<string> _header = [];
    private readonly List<IEnumerable<PayloadData>> _body = [];

    public bool IsClosed { get; private set; }

    public int WrittenCount => _body.Count;

    public string FilePath => path;

    public string RelativePath => relativePath;

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

        _body.Add(items);
    }

    public void RemoveLineBody(int count)
    {
        if (count >= _body.Count)
        {
            _body.Clear();
            return;
        }

        _body.RemoveRange(_body.Count - count, count);
    }

    public async Task SaveAsync()
    {
        if (IsClosed)
        {
            return;
        }

        using StreamWriter sw = new(path);
        await sw.WriteLineAsync(string.Join(",", _header.Select(VaildCsv))).ConfigureAwait(false);

        foreach (var items in _body)
        {
            var first = items.First();

            // payload 数据非数组
            if (!first.IsArray())
            {
                await sw.WriteLineAsync(string.Join(",", items.Select(s => VaildCsv(s.GetString())))).ConfigureAwait(false);
            }
            else
            {
                // payload 数据为数组
                List<string[]> matrix = new(_header.Count);
                foreach (var header in _header)
                {
                    var payload = items.First(s => s.GetExtraValue<string>("DisplayName") == header);
                    matrix.Add(payload.GetStringArray());
                }

                for (var i = 0; i < matrix[0].Length; i++)
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
