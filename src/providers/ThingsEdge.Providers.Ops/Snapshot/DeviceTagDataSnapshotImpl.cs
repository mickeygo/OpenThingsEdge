using ThingsEdge.Router.Devices;

namespace ThingsEdge.Providers.Ops.Snapshot;

internal sealed class DeviceTagDataSnapshotImpl(ITagDataSnapshot tagDataSnapshot) : IDeviceTagDataSnapshot, ISingletonDependency
{
    public PayloadData? Get(string tagId) => tagDataSnapshot.Get(tagId)?.Data;
}
