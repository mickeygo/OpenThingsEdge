namespace ThingsEdge.Providers.Kepware;

public sealed class KepwareDriverProvider : IDriverProvider
{
    public KepwareDriverProvider()
    {
        
    }

    public IDataCommand Command => new KepwareDataCommand();

    public bool IsEngine => false;

    public Task RunAsync()
    {
        throw new NotImplementedException();
    }

    public Task ShutdownAsync()
    {
        throw new NotImplementedException();
    }
}
