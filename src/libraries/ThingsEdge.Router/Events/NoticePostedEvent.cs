namespace ThingsEdge.Router.Events;

/// <summary>
/// 通知消息发送事件。
/// </summary>
/// <param name="Message">请求消息</param>
/// <param name="LastMasterPayload">上一次触发点的主数据值，若是第一次请求，会为 null。</param>
public sealed record NoticePostedEvent(RequestMessage Message, PayloadData? LastMasterPayload) : INotification;
