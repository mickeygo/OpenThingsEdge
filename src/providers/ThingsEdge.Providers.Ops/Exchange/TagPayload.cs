namespace ThingsEdge.Providers.Ops.Exchange;

public sealed class TagPayload
{
    public bool Ok { get; init; }

    [NotNull]
    public string? Error { get; init; }

    [NotNull]
    public PayloadData? Payload { get; set; }
}
