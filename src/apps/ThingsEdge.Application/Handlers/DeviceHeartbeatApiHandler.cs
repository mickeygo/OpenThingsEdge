using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Management.Equipment;
using ThingsEdge.Application.Models;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application.Handlers;

internal sealed class DeviceHeartbeatApiHandler : IDeviceHeartbeatApi
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public DeviceHeartbeatApiHandler(IServiceProvider serviceProvider, ILogger<DeviceHeartbeatApiHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ChangeAsync(string channelName, Device device, Tag tag, bool isOnline, CancellationToken cancellationToken)
    {
        EquipmentCodeInput input = new()
        {
            Line = channelName,
            EquipmentCodes = new(),
        };

        var tagGroup = device.GetTagGroup(tag.TagId);
        if (tagGroup is null)
        {
            // 心跳是跟随设备设定的
            input.EquipmentCodes.AddRange(device.TagGroups.Select(s => s.Name));
        }
        else
        {
            // 心跳对应具体的分组
            input.EquipmentCodes.Add(tagGroup.Name);
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var equipmentStateManager = scope.ServiceProvider.GetRequiredService<EquipmentStateManager>();

            // 更改设备运行状态（Running/Offline）
            var runningState = isOnline ? EquipmentRunningState.Running : EquipmentRunningState.Offline;
            await equipmentStateManager.ChangeStateAsync(input, runningState, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeviceHeartbeatApiHandler] 心跳数据预处理异常。");
        }
    }
}
