using SqlSugar;

namespace ThingsEdge.Application.Infrastructure;

/// <summary>
/// SqlSugar 具有多租户策略的仓储类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlSugarRepository<T> : SimpleClient<T> where T : class, new()
{
    protected readonly ITenant Tenant;

    public SqlSugarRepository(ISqlSugarClient context) : base(context)
    {
        Tenant = context.AsTenant(); // 采用租户策略
        base.Context = Tenant.GetConnectionScopeWithAttr<T>(); // 线程安全，作用域 Scope，根据实体配置的 <TenantAttribute> 使用对应的 DB。
    }
}
