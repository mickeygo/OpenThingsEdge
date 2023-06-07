namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 软删除接口过滤器
/// </summary>
internal interface IDeletedFilter
{
    /// <summary>
    /// 软删除
    /// </summary>
    bool IsDelete { get; set; }
}

/// <summary>
/// 租户Id接口过滤器
/// </summary>
internal interface ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    long? TenantId { get; set; }
}
