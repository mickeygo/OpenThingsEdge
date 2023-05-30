using ThingsEdge.Application.Configuration;
using ThingsEdge.Application.Domain.Services;
using ThingsEdge.Application.Dtos;
using ThingsEdge.Application.Management.Equipment;
using ThingsEdge.Application.Models;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Interfaces;

namespace ThingsEdge.Application.Handlers;

internal class MessageRequestPostingApiHandler : IMessageRequestPostingApi
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IAlarmService _alarmService;
    private readonly EquipmentStateManager _equipmentStateManager;
    private readonly IEntryService _entryService;
    private readonly IArchiveService _archiveService;
    private readonly ApplicationConfig _appConfig;
    private readonly ILogger _logger;

    public MessageRequestPostingApiHandler(IServiceProvider serviceProvider, 
        IAlarmService alarmService,
        EquipmentStateManager equipmentStateManager, 
        IEntryService entryService,
        IArchiveService archiveService,
        IOptions<ApplicationConfig> appConfig,
        ILogger<MessageRequestPostingApiHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _alarmService = alarmService;
        _equipmentStateManager = equipmentStateManager;
        _entryService = entryService;
        _archiveService = archiveService;
        _appConfig = appConfig.Value;
        _logger = logger;
    }

    public async Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken)
    {
        var self = requestMessage.Self();

        try
        {
            if (requestMessage.Flag == TagFlag.Notice)
            {
                if (self.TagName == _appConfig.TagDefine.PLC_Alarm)  // 设备警报
                {
                    var lastAlarms = lastMasterPayloadData?.GetBitArray();
                    var newAlarms = self.GetBitArray();

                    await _alarmService.RecordAlarmsAsync(new AlarmInput { Line = requestMessage.Schema.ChannelName, LastAlarms = lastAlarms, NewAlarms = newAlarms });
                }
                else if (self.TagName == _appConfig.TagDefine.PLC_Equipment_State) // 设备运行状态
                {
                    int state = self.GetInt(); // 转换可能异常
                    if (Enum.IsDefined(typeof(EquipmentRunningState), state))
                    {
                        await _equipmentStateManager.ChangeStateAsync(GetEquipmentCodeInput(), (EquipmentRunningState)state, cancellationToken);
                    }
                }
                else if (self.TagName == _appConfig.TagDefine.PLC_Equipment_Mode)  // 设备运行模式
                {
                    int mode = self.GetInt();
                    if (Enum.IsDefined(typeof(EquipmentRunningMode), mode))
                    {
                        await _equipmentStateManager.ChangeModeAsync(GetEquipmentCodeInput(), (EquipmentRunningMode)mode, cancellationToken);
                    }                  
                }
            }

            if (requestMessage.Flag == TagFlag.Trigger)
            {
                if (self.TagName == _appConfig.TagDefine.PLC_Entry_Sign) // 产品进站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(_appConfig.TagDefine.PLC_Entry_SN)?.GetString();
                    if (!string.IsNullOrEmpty(sn))
                    {
                        await _entryService.EntryAsync(requestMessage.Schema.ChannelName, station, sn);
                    }
                }
                else if (self.TagName == _appConfig.TagDefine.PLC_Archive_Sign) // 产品出站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(_appConfig.TagDefine.PLC_Archive_SN)?.GetString();
                    if (!string.IsNullOrEmpty(sn))
                    {
                        await _archiveService.ArchiveAsync(requestMessage.Schema.ChannelName, station, sn);
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
            var deviceManager = _serviceProvider.GetRequiredService<IDeviceManager>();
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
