﻿namespace ThingsEdge.Router.Interfaces;

/// <summary>
/// 设备请求预处理接口。
/// </summary>
public interface IDirectMessageRequestApi
{
    /// <summary>
    /// 请求消息。
    /// </summary>
    /// <param name="lastMasterPayloadData">最近一次记录的标记主数据，没有则为null</param>
    /// <param name="requestMessage">请求消息</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PostAsync(PayloadData? lastMasterPayloadData, RequestMessage requestMessage, CancellationToken cancellationToken);
}
