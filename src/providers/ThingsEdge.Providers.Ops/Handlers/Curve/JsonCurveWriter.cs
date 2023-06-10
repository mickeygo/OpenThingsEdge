using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace ThingsEdge.Providers.Ops.Handlers.Curve;

/// <summary>
/// 保存为 JSON 文件的写入器。
/// </summary>
internal sealed class JsonCurveWriter : ICurveWriter
{
    private readonly InternalJsonCurve _jsonCurve = new();

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

        _jsonCurve.Header.AddRange(header);
    }

    public void WriteLineBody(IEnumerable<PayloadData> item)
    {
        if (IsClosed)
        {
            return;
        }

        ++WrittenCount;
        _jsonCurve.Body.Add(item);
    }

    public async Task SaveAsync()
    {
        if (IsClosed)
        {
            return;
        }

        var content = JsonSerializer.Serialize(_jsonCurve, new JsonSerializerOptions
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

    class InternalJsonCurve
    {
        public List<string> Header { get; } = new();

        public List<IEnumerable<PayloadData>> Body { get; } = new();
    }
}
