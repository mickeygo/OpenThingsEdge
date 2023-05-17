namespace ThingsEdge.Router.Forwarder;

/// <summary>
/// Forward 路由。
/// </summary>
public interface IForwardRouter
{
    /// <summary>
    /// 构建路由。
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="masterTag">主标记。</param>
    /// <returns></returns>
    public string Make(Schema schema, Tag masterTag)
    {
        return masterTag.Flag switch
        {
            TagFlag.Notice => "/api/iotgateway/notice",
            TagFlag.Trigger => "/api/iotgateway/trigger",
            _ => "",
        };
    }
}
