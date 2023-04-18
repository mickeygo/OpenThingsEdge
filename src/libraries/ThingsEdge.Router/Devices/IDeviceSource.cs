﻿using ThingsEdge.Contracts.Devices;

namespace ThingsEdge.Router.Devices;

/// <summary>
/// 设备数据来源
/// </summary>
public interface IDeviceSource
{
    /// <summary>
    /// 获取通道数据。
    /// </summary>
    /// <returns></returns>
    List<Channel> GetChannels();
}