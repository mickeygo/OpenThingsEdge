namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// 转发请求数据源分类
/// </summary>
public enum ForworderSource
{
    None = 0,

    HTTP,

    MQTT,

    Native,
}
