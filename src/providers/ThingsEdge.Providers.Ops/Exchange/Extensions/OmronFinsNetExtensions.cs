using Ops.Communication.Profinet.Omron;
using ThingsEdge.Contracts.Variables;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// <see cref="OmronFinsNet"/> 扩展。
/// </summary>
public static class OmronFinsNetExtensions
{
    public static Task<(bool ok, List<PayloadData>? data, string? err)> ReadMultiAsync(this OmronFinsNet omronFinsNet, IEnumerable<Tag> tags)
    {
        throw new NotImplementedException();
    }
}
