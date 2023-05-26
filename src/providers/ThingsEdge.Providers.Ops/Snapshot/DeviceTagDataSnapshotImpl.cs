using ThingsEdge.Router.Devices;

namespace ThingsEdge.Providers.Ops.Snapshot;

internal sealed class DeviceTagDataSnapshotImpl : IDeviceTagDataSnapshot
{
    private readonly ITagDataSnapshot _tagDataSnapshot;

    public DeviceTagDataSnapshotImpl(ITagDataSnapshot tagDataSnapshot) => _tagDataSnapshot = tagDataSnapshot;

    public PayloadData? Get(string tagId) => _tagDataSnapshot.Get(tagId)?.Data;
}
