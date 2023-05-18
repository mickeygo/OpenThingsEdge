using ThingsEdge.Application.Contract;
using ThingsEdge.Application.Domain.Services;
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

    public async Task PostAsync(PayloadData? oldMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken)
    {
        var self = requestMessage.Self();

        if (requestMessage.Flag == TagFlag.Notice)
        {
            try
            {
                if (self.TagName == TagDefineConstants.PLC_Alarm)  // 设备警报
                {
                    var oldAlarms = oldMasterPayloadData?.GetBitArray() ?? Array.Empty<bool>();
                    var newAlarms = self.GetBitArray();

                    var alarmService = _serviceProvider.GetRequiredService<IAlarmService>();
                    await alarmService.RecordAlarmsAsync(oldAlarms, newAlarms);
                }
                else if (self.TagName == TagDefineConstants.PLC_Equipment_State) // 设备运行状态
                {
                    var state = (EquipmentRunningState)self.GetInt();
                    var equipManager = _serviceProvider.GetRequiredService<EquipmentStateManager>();
                    await equipManager.ChangeStateAsync(GetEquipmentCodes(), state, cancellationToken);
                }
                else if (self.TagName == TagDefineConstants.PLC_Equipment_Mode)  // 设备运行模式
                {
                    var mode = (EquipmentRunningMode)self.GetInt();
                    var equipManager = _serviceProvider.GetRequiredService<EquipmentStateManager>();
                    await equipManager.ChangeModeAsync(GetEquipmentCodes(), mode, cancellationToken);
                }
                else if (self.TagName == TagDefineConstants.PLC_Entry_Sign) // 产品进站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(TagDefineConstants.PLC_Entry_SN)!.GetString();
                }
                else if (self.TagName == TagDefineConstants.PLC_Archive_Sign) // 产品出站
                {
                    var station = requestMessage.Schema.TagGroupName!;
                    var sn = requestMessage.GetData(TagDefineConstants.PLC_Archive_SN)!.GetString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MessageRequestPostingApiHandler] 请求数据预处理异常。");
            }
        }

        string[] GetEquipmentCodes()
        {
            if (!string.IsNullOrEmpty(requestMessage.Schema.TagGroupName))
            {
                return new string[] { requestMessage.Schema.TagGroupName };
            }

            // 非分组，归属于设备
            // 查找设备对所有分组，

            return new string[] { requestMessage.Schema.DeviceName };
        }
    }
}
