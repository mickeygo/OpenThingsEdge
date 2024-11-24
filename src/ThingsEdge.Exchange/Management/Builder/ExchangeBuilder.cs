namespace ThingsEdge.Exchange;

/// <summary>
/// ExchangeBuilder 实例
/// </summary>
/// <param name="builder"></param>
internal sealed class ExchangeBuilder(IHostBuilder builder) : IExchangeBuilder
{
    public IHostBuilder Builder { get; } = builder ?? throw new ArgumentNullException(nameof(builder));
}
