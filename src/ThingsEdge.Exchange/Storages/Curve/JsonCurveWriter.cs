using System.Text.Encodings.Web;
using System.Text.Unicode;
using ThingsEdge.Exchange.Contracts;

namespace ThingsEdge.Exchange.Storages.Curve;

/// <summary>
/// 曲线保存为 JSON 文件的写入器。
/// </summary>
internal sealed class JsonCurveWriter(string path) : ICurveWriter
{
    static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs), // 中日韩统一表意文字（CJK Unified Ideographs）
    };

    private readonly InternalCurveData _curveData = new();

    public bool IsClosed { get; private set; }

    public long WrittenCount { get; private set; }

    public string FilePath => path;

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

        Dictionary<string, double[]> dict = new(_curveData.Header.Count);
        foreach (var header in _curveData.Header)
        {
            var items = _curveData.Body
                .SelectMany(s => s)
                .Where(s => s.GetExtraValue<string>("DisplayName") == header)
                .Select(GetDoubleArray).ToArray();

            dict[header] = items;
        }

        var content = JsonSerializer.Serialize(dict, s_jsonOptions);
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

    private static double GetDoubleArray(PayloadData payload)
    {
        if (payload.TryGetAsDouble(out var value))
        {
            return value.Value;
        }

        return 0;
    }

    sealed class InternalCurveData
    {
        public List<string> Header { get; } = [];

        public List<IEnumerable<PayloadData>> Body { get; } = [];
    }
}
