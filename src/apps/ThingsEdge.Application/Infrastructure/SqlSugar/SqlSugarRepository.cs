using SqlSugar;

namespace ThingsEdge.Application.Infrastructure;

/// <summary>
/// SqlSugar仓储类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlSugarRepository<T> : SimpleClient<T> where T : class, new()
{
    protected ITenant? Tenant = null;

    public SqlSugarRepository(ISqlSugarClient? context = null) : base(context) // 默认值等于null不能少
    {
        Tenant = ((SqlSugarScope)context!).AsTenant(); // 采用租户策略
        base.Context = Tenant.GetConnectionWithAttr<T>(); // 根据实体配置的 <TenantAttribute> 切换租户
    }
}
