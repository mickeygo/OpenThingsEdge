using ThingsEdge.Exchange.Configuration;
using ThingsEdge.Exchange.Contracts;
using ThingsEdge.Exchange.Engine.Connectors;
using ThingsEdge.Exchange.Engine.Messages;
using ThingsEdge.Exchange.Engine.Snapshot;
using ThingsEdge.Exchange.Forwarders;
using ThingsEdge.Exchange.Storages.Curve;

namespace ThingsEdge.Exchange.Engine.Handlers;

/// <summary>
/// 开关消息处理器。
/// </summary>
internal sealed class SwitchMessageHandler(
    CurveStorage curveStorage,
    ISwitchForwarderProxy switchForwarderProxy,
    ITagDataSnapshot tagDataSnapshot,
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
            };
            reqMessage.Values.Add(message.Self!);

            // 快照设置
            tagDataSnapshot.Change(message.Self!);

            if (message.State == SwitchState.On)
            {
                // 先查找主数据
                List<PayloadData> masters = [];
                var masterTags = message.Tag.NormalTags.Where(s => s.GetExtraValue("CurveUsage") == "Master").ToList();
                if (masterTags.Count > 0)
                {
                    var (ok0, masterPayloads, err0) = await message.Connector.ReadMultiAsync(masterTags, options.Value.AllowReadMultiple).ConfigureAwait(false);
                    if (ok0)
                    {
                        masters = masterPayloads!; // 都以文本记录
                    }
                    else
                    {
                        logger.LogError("[Switch] 读取 Master 标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {Address}, 错误: {Err}",
                                message.Device.Name, message.Tag.Name, message.Tag.Address, err0);
                    }
                }

                // 获取或创建数据写入器（即使主数据读取失败也继续）
                // 先创建写入器
                CurveModel model = new(message.ChannelName, message.Device.Name, tagGroup?.Name, message.Tag.GetExtraValue("DisplayName"), masters);
                var(ok1, err1) = curveStorage.CreateIfNotExists(message.Tag.TagId, model);
                if (!ok1)
                {
                    logger.LogError("[Switch] Curve 写入器创建失败, 设备: {DeviceName}, 标记: {TagName}，错误: {Err}",
                        message.Device.Name, message.Tag.Name, err1);
                }

                // 添加头信息
                var header = message.Tag.NormalTags
                    .Where(s => s.GetExtraValue("CurveUsage") == "Data")
                    .Select(s => s.GetExtraValue("DisplayName") ?? "");
                var (ok2, err2) = curveStorage.WriteHeader(message.Tag.TagId, header);
                if (!ok2)
                {
                    logger.LogError("[Switch] 文件写入头失败, 设备: {DeviceName}, 标记: {TagName}，错误: {Err}",
                        message.Device.Name, message.Tag.Name, err2);
                }
            }
            else if (message.State == SwitchState.Off)
            {
                var (ok3, curveModel, path) = await curveStorage.SaveAsync(message.Tag.TagId).ConfigureAwait(false);
                if (ok3)
                {
                    // 发送曲线通知消息
                    await switchForwarderProxy.PublishAsync(new SwitchContext(
                        reqMessage,
                        curveModel!.CurveName,
                        curveModel.Masters,
                        path), cancellationToken).ConfigureAwait(false);
                }
            }

            return;
        }

        // 开关中的具体数据
        var (ok4, err4) = curveStorage.CanWriteBody(message.Tag.TagId);
        if (!ok4)
        {
            // 没有设置错误消息时，不记录日志
            if (!string.IsNullOrEmpty(err4))
            {
                logger.LogError("[Switch] 文件写入数据失败，设备: {DeviceName}, 标记: {TagName}，地址: {Address}，错误：{Err}",
                    message.Device.Name, message.Tag.Name, message.Tag.Address, err4);
            }

            return;
        }

        // 读取触发标记下的子数据。
        var curveTags = message.Tag.NormalTags.Where(s => s.GetExtraValue("CurveUsage") == "Data").ToList();
        if (curveTags.Count > 0)
        {
            var (ok5, normalPaydatas, err5) = await message.Connector.ReadMultiAsync(curveTags, options.Value.AllowReadMultiple).ConfigureAwait(false);
            if (!ok5)
            {
                logger.LogError("[Switch] 批量读取子标记值失败, 设备: {Name}, 错误: {Err}", message.Device.Name, err5);
                return;
            }

            var (ok6, err6) = curveStorage.WriteLine(message.Tag.TagId, normalPaydatas!);
            if (!ok6)
            {
                logger.LogError("[Switch] 文件写入数据失败, 设备: {DeviceName}, 标记: {TagName}，错误: {Err}",
                    message.Device.Name, message.Tag.Name, err6);
            }
        }
    }
}
