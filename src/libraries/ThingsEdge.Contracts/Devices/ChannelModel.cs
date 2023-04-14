﻿namespace ThingsEdge.Contracts.Devices;

/// <summary>
/// Channel 模型。
/// </summary>
public enum ChannelModel
{
    Modbus = 1,

    Siemens,

    Melsec,

    Omron,

    AllenBradley,
}
