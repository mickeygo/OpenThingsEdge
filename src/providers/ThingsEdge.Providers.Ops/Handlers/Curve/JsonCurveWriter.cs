using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 保存为 JSON 文件的写入器。
/// </summary>
internal sealed class JsonCurveWriter : ICurveWriter
{
    private readonly InternalCurveData _curveData = new();

    public bool IsClosed { get; private set; }

    public long WrittenCount { get; private set; }

    public string FilePath { get; init; } = null!;

    public JsonCurveWriter(string path)
    {
        FilePath = path;
    }

    public void WriteHeader(IEnumerable<string> header)
    {
        if (IsClosed)
        {
            return;
        }

        _curveData.Header.AddRange(header);
    }

    public void WriteLineBody(IEnumerable<PayloadData> items)
    {
        if (IsClosed)
        {
            return;
        }

        ++WrittenCount;
        _curveData.Body.Add(items);
    }

    public async Task SaveAsync()
    {
        if (IsClosed)
        {
            return;
        }

        // 转换为 json 格式：
        // {
        //   "name1": [v1, v2],
        //   "name2": [v1, v2],
        // }

        Dictionary<string, string[]> dict = new(_curveData.Header.Count);
        foreach (var header in _curveData.Header)
        {
            var items = _curveData.Body
                .SelectMany(s => s)
                .Where(s => s.DisplayName == header)
                .OrderBy(s => s.CreatedTime)
                .SelectMany(s => s.GetStringArray()).ToArray();

            dict[header] = items;
        }

        var content = JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs), // 中日韩统一表意文字（CJK Unified Ideographs）
        });

        using StreamWriter sw = new(FilePath);
        await sw.WriteAsync(content).ConfigureAwait(false);
    }

    public void Close()
    {
        if (!IsClosed)
        {
            IsClosed = true;
        }
    }

    class InternalCurveData
    {
        public List<string> Header { get; } = new();

        public List<IEnumerable<PayloadData>> Body { get; } = new();
    }
}
