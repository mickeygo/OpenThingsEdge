namespace ThingsEdge.Providers.Kepware.IotGateway.RESTful;

/// <summary>
/// IotGateway RESTful API 接口。
/// </summary>
public interface IRESTServerApi
{
    Task<ReadResult?> ReadAsync(string[] tags, CancellationToken cancellationToken = default);

    Task<WriteResultCollection?> WriteAsync(Dictionary<string, object> value, CancellationToken cancellationToken = default);
}
