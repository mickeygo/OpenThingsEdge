namespace ThingsEdge.Providers.Ops;

public sealed class OpsDriverProvider : IDriverProvider
{
    public bool IsEngine => true;

    public OpsDriverProvider()
    {
        
    }

    public IDataCommand Command => throw new NotImplementedException();

    public Task RunAsync()
    {
        throw new NotImplementedException();
    }

    public Task ShutdownAsync()
    {
        throw new NotImplementedException();
    }
}
