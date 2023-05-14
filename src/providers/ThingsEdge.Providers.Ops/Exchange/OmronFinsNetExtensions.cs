using Ops.Communication.Profinet.Omron;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// <see cref="OmronFinsNet"/> 扩展。
/// </summary>
public static class OmronFinsNetExtensions
{
    public static async Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(this OmronFinsNet omronFinsNet, IEnumerable<Tag> tags)
    {
        List<PayloadData> list = new();

        return (true, list, default);
    }
}
