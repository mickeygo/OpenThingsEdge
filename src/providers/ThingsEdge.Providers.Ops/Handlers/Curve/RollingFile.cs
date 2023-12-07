namespace ThingsEdge.Providers.Ops.Handlers.Curve;

internal sealed class RollingFile
{
    private readonly string _rootPath;
    private readonly long _retainedSizeLimit;
    private long _size;

    public RollingFile(string rootPath, long retainedSizeLimit, long stepSizeLimit)
    {
        _rootPath = rootPath;
        _retainedSizeLimit = retainedSizeLimit;
        Init();
    }

    public void Add(long size)
    {
        _size += size;
        if (_size > _retainedSizeLimit)
        {
            // 移除文件
        }
    }

    private void Init()
    {
        if (Directory.Exists(_rootPath))
        {
            DirectoryInfo di = new(_rootPath);
            _size = di.GetFiles("*", SearchOption.AllDirectories).Sum(s => s.Length);
        }
    }
}
