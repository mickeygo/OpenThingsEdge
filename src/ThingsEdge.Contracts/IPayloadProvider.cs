namespace ThingsEdge.Contracts;

public interface IPayloadProvider
{
    Task<Payload> MakePayloadAsync(Payload payload);
}
