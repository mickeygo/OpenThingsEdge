using ThingsEdge.Application.Contract;
using ThingsEdge.Application.Domain.Services;
using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Management.Equipment;
using ThingsEdge.Application.Models;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application.Handlers;

internal class MessageRequestPostingApiHandler : IMessageRequestPostingApi
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public MessageRequestPostingApiHandler(IServiceProvider serviceProvider, ILogger<MessageRequestPostingApiHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken)
    {
        var self = requestMessage.Self();

        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            if (requestMessage.Flag == TagFlag.Notice)
            {
                if (self.TagName == TagDefineConstants.PLC_Alarm)  // 设备警报
                {
                    var lastAlarms = lastMasterPayloadData?.GetBitArray();
                    var newAlarms = self.GetBitArray();

                    var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();
                    await alarmService.RecordAlarmsAsync(new AlarmInput { Line = requestMessage.Schema.ChannelName, LastAlarms = lastAlarms, NewAlarms = newAlarms });
                }
                else if (self.TagName == TagDefineConstants.PLC_Equipment_State) // 设备运行状态
                {
                    int state = self.GetInt(); // 转换可能异常
                    if (Enum.IsDefined(typeof(EquipmentRunningState), state))
                    {
                        var equipManager = scope.ServiceProvider.GetRequiredService<EquipmentStateManager>();
                        await equipManager.ChangeStateAsync(GetEquipmentCodeInput(), (EquipmentRunningState)state, cancellationToken);
                    }
                }
                else if (self.TagName == TagDefineConstants.PLC_Equipment_Mode)  // 设备运行模式
                {
                    int mode = self.GetInt();
                    if (Enum.IsDefined(typeof(EquipmentRunningMode), mode))
                    {
                        var equipManager = scope.ServiceProvider.GetRequiredService<EquipmentStateManager>();
                        await equipManager.ChangeModeAsync(GetEquipmentCodeInput(), (EquipmentRunningMode)mode, cancellationToken);
                    }                  
                }
            }

            if (requestMessage.Flag == TagFlag.Trigger)
            {
                if (self.TagName == TagDefineConstants.PLC_Entry_Sign) // 产品进站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(TagDefineConstants.PLC_Entry_SN)!.GetString();
                    if (!string.IsNullOrEmpty(sn))
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IEntryService>();
                        await service.EntryAsync(requestMessage.Schema.ChannelName, station, sn);
                    }
                }
                else if (self.TagName == TagDefineConstants.PLC_Archive_Sign) // 产品出站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(TagDefineConstants.PLC_Archive_SN)!.GetString();
                    if (!string.IsNullOrEmpty(sn))
                    {
                        var service = scope.ServiceProvider.GetRequiredService<IArchiveService>();
                        await service.ArchiveAsync(requestMessage.Schema.ChannelName, station, sn);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MessageRequestPostingApiHandler] 请求数据预处理异常。");
        }

        EquipmentCodeInput GetEquipmentCodeInput()
        {
            EquipmentCodeInput input = new()
            {
                Line = requestMessage.Schema.ChannelName,
                EquipmentCodes = new(),
            };

            // 标记归属于分组
            if (!string.IsNullOrEmpty(requestMessage.Schema.TagGroupName))
            {
                input.EquipmentCodes.Add(requestMessage.Schema.TagGroupName);
                return input;
            }

            // 标记归属于设备，查找设备下所有分组
            var deviceManager = scope.ServiceProvider.GetRequiredService<IDeviceManager>();
            var device = deviceManager.GetDevice(requestMessage.Schema.ChannelName, requestMessage.Schema.DeviceName);
            if (device != null)
            {
                input.EquipmentCodes.AddRange(device.TagGroups.Select(s => s.Name));
                return input;
            }

            // 若设备下没有分组，直接用设备名称。
            input.EquipmentCodes.Add(requestMessage.Schema.DeviceName);

            return input;
        }
    }
}
