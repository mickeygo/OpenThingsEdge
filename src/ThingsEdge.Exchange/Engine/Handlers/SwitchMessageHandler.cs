using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Forwarders;
using ThingsEdge.Exchange.Storages.Curve;

namespace ThingsEdge.Exchange.Engine.Handlers;

/// <summary>
/// 开关消息处理器。
/// </summary>
internal sealed class SwitchMessageHandler(
    CurveStorage curveStorage,
    ISwitchForwarderProxy switchForwarderProxy,
    IOptions<ExchangeOptions> options,
    ILogger<SwitchMessageHandler> logger) : ISwitchMessageHandler
{
    public async Task HandleAsync(SwitchMessage message, CancellationToken cancellationToken)
    {
        // 开关信号
        if (message.IsSwitchSignal)
        {
            var tagGroup = message.Device.GetTagGroup(message.Tag.TagId);
            var reqMessage = new RequestMessage
            {
                Schema = new()
                {
                    ChannelName = message.ChannelName,
                    DeviceName = message.Device.Name,
                    TagGroupName = tagGroup?.Name,
                },
                Flag = message.Tag.Flag,
            };
            reqMessage.Values.Add(message.Self!);

            if (message.State == SwitchState.On)
            {
                string? sn = null, no = null;
                // 码和序号
                var snTag = message.Tag.NormalTags.FirstOrDefault(s => s.GetExtraValue("CurveUsage") == "SwitchSN");
                if (snTag != null)
                {
                    var (ok1, data1, err1) = await message.Connector.ReadAsync(snTag).ConfigureAwait(false);
                    if (ok1)
                    {
                        sn = data1!.GetString();
                        reqMessage.Values.Add(data1!);
                    }
                    else
                    {
                        logger.LogError("[Switch] 读取 SwitchSN 标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                            message.Device.Name, message.Tag.Name, message.Tag.Address, err1);
                    }
                }

                var noTag = message.Tag.NormalTags.FirstOrDefault(s => s.GetExtraValue("CurveUsage") == "SwitchNo");
                if (noTag != null)
                {
                    var (ok3, data3, err3) = await message.Connector.ReadAsync(noTag).ConfigureAwait(false);
                    if (ok3)
                    {
                        no = data3!.GetString();
                        reqMessage.Values.Add(data3!);
                    }
                    else
                    {
                        logger.LogError("[Switch] 读取 SwitchNo 标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                            message.Device.Name, message.Tag.Name, message.Tag.Address, err3);
                    }
                }

                // 获取或创建数据写入器（即使 sn 和 no 读取失败继续）
                try
                {
                    CurveModel model = new()
                    {
                        Barcode = sn,
                        No = no,
                        CurveName = message.Tag.GetExtraValue("DisplayName"),
                        ChannelName = message.ChannelName,
                        DeviceName = message.Device.Name,
                        GroupName = tagGroup?.Name,
                    };
                    var writer = curveStorage.GetOrCreate(message.Tag.TagId, model);

                    // 添加头信息
                    var header = message.Tag.NormalTags
                        .Where(s => s.GetExtraValue("CurveUsage") == "SwitchCurve")
                        .Select(s => s.GetExtraValue("DisplayName") ?? "");
                    writer.WriteHeader(header);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Switch] Curve Storage 创建或写入头信息失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}",
                        message.Device.Name, message.Tag.Name, message.Tag.Address);
                }
            }
            else if (message.State == SwitchState.Off)
            {
                var (ok4, curveModel, path) = curveStorage.Save(message.Tag.TagId);
                if (ok4)
                {
                    // 发送曲线通知消息
                    await switchForwarderProxy.PublishAsync(new SwitchContext(
                        curveModel!.ChannelName,
                        curveModel.DeviceName,
                        curveModel.GroupName,
                        curveModel.Barcode ?? "",
                        curveModel!.No,
                        curveModel.CurveName,
                        path), cancellationToken).ConfigureAwait(false);
                }
            }

            return;
        }

        // 开关具体数据
        var (ok, err, writer2) = curveStorage.CanWriteBody(message.Tag.TagId);
        if (!ok)
        {
            // 没有设置错误消息时，不记录日志
            if (!string.IsNullOrEmpty(err))
            {
                logger.LogError("[Switch] 错误：{Err}, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}",
                    err, message.Device.Name, message.Tag.Name, message.Tag.Address);
            }

            return;
        }

        // 读取触发标记下的子数据。
        var curveTags = message.Tag.NormalTags;
        if (curveTags.Count > 0)
        {
            var (ok2, normalPaydatas, err2) = await message.Connector.ReadMultiAsync(curveTags, options.Value.AllowReadMultiple).ConfigureAwait(false);
            if (!ok2)
            {
                logger.LogError("[Switch] 批量读取子标记值失败, 设备: {Name}, 错误: {Err}", message.Device.Name, err2);
                return;
            }

            try
            {
                writer2!.WriteLineBody(normalPaydatas!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Switch] 曲线数据写入文件失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}",
                    message.Device.Name, message.Tag.Name, message.Tag.Address);
            }
        }
    }
}
