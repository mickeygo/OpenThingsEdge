using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops;

public sealed class OpsDriverProvider : IDriverProvider
{
    private readonly IEngine _engine;
    private readonly IDataCommand _command;

    public bool IsEngine => true;

    public OpsDriverProvider(IEngine engine, IDataCommand command)
    {
        _engine = engine;
        _command = command;

    }

    public IDataCommand Command => _command;

    public async Task RunAsync()
    {
        await _engine.StartAsync();
    }

    public Task ShutdownAsync()
    {
        _engine.Stop();
        return Task.CompletedTask;
    }
}
