namespace ThingsEdge.Contrib.DotNetty;

public abstract class AbstractHttpServerHandler
{
    public virtual Task HandleAsync(string uri)
    {
        return Task.CompletedTask;
    }
}
