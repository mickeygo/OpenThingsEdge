using ThingsEdge.Application.Management.Equipment;
using ThingsEdge.Application.Models;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application.Handlers;

internal sealed class DeviceHeartbeatApiHandler : IDeviceHeartbeatApi
{
    private readonly EquipmentStateManager _equipmentStateManager;

    public DeviceHeartbeatApiHandler(EquipmentStateManager equipmentStateManager)
    {
        _equipmentStateManager = equipmentStateManager;
    }

    public async Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken)
    {
        List<string> equipments = new();

        var tagGroup = device.GetTagGroup(tag.TagId);
        if (tagGroup is null)
        {
            // 心跳是跟随设备设定的
            equipments.AddRange(device.TagGroups.Select(s => s.Name));
        }
        else
        {
            // 心跳对应具体的分组
            equipments.Add(tagGroup.Name);
        }

        try
        {
            // 更改设备运行状态（Running/Offline）
            var runningState = isOnline ? EquipmentRunningState.Running : EquipmentRunningState.Offline;
            await _equipmentStateManager.ChangeStateAsync(equipments, runningState, cancellationToken);
        }
        catch (Exception)
        {
        }
    }
}
