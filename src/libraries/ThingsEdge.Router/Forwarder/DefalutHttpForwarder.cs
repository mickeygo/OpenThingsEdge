namespace ThingsEdge.Router.Forwarder;

internal sealed class DefalutHttpForwarder : IHttpForwarder
{
    public Task<HttpResult> SendAsync(RequestMessage message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
