using SqlSugar;
using Yitter.IdGenerator;

namespace ThingsEdge.Application.Infrastructure;

/// <summary>
/// SqlSugar 配置。
/// </summary>
public static class SqlSugarSetup
{
    /// <summary>
    /// Sqlsugar 上下文初始化
    /// </summary>
    /// <param name="services"></param>
    public static void AddSqlSugarSetup(this IServiceCollection services)
    {
        // 设置雪花Id算法机器码
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(1)); // 不同应用的配置文件值不同， SnowFlakeSingle 长度 19，偏长

        services.AddSingleton<ISqlSugarClient>(sp => MakeSqlSugarScope(sp, services)); // 需注册为 Singleton
        services.AddScoped(typeof(SqlSugarRepository<>)); // 仓储注册
    }

    private static SqlSugarScope MakeSqlSugarScope(IServiceProvider sp, IServiceCollection services)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SqlSugarSetup));

        var configuration = sp.GetRequiredService<IConfiguration>();
        var dbOptions = configuration.GetSection("DbConnection").Get<DbConnectionOptions>();
        var configureExternalServices = new ConfigureExternalServices
        {
            EntityService = (type, column) => // 修改列可空 1、带?问号；2、String类型若没有Required
            {
                if ((type.PropertyType.IsGenericType && type.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    || (type.PropertyType == typeof(string) && type.GetCustomAttribute<RequiredAttribute>() == null))
                {
                    column.IsNullable = true;
                }
            }
        };
        dbOptions!.ConnectionConfigs?.ForEach(config =>
        {
            config.ConfigureExternalServices = configureExternalServices;
        });

        return new SqlSugarScope(dbOptions.ConnectionConfigs, db =>
        {
            dbOptions.ConnectionConfigs?.ForEach(config =>
            {
                // GetConnectionScope 与 GetConnectionScopeWithAttr 对应
                var dbProvider = db.GetConnectionScope((string)config.ConfigId);

                // 设置超时时间
                dbProvider.Ado.CommandTimeOut = 30;

                if (dbOptions.EnabledSqlLog)
                {
                    // 打印SQL语句
                    dbProvider.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        logger.LogInformation("【执行SQL】{NewLine} {SQL}", Environment.NewLine, UtilMethods.GetSqlString(config.DbType, sql, pars));
                    };

                    dbProvider.Aop.OnError = (ex) =>
                    {
                        logger.LogError("【错误SQL】{Message} {NewLine} {SQL}",
                            ex.Message, Environment.NewLine, UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres));
                    };
                }

                // 数据审计
                dbProvider.Aop.DataExecuting = (oldValue, entityInfo) =>
                {
                    // 新增操作
                    if (entityInfo.OperationType == DataFilterType.InsertByObject)
                    {
                        // 主键(long类型)且没有值的---赋值雪花Id
                        if (entityInfo.EntityColumnInfo.IsPrimarykey && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                        {
                            var id = oldValue;
                            if (id is null || (long)id == 0)
                            {
                                entityInfo.SetValue(YitIdHelper.NextId());
                            }
                        }

                        // 自动设置创建时间，这里更新时间一起设置。
                        if (entityInfo.PropertyName == nameof(EntityBase.CreateTime) || entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                        {
                            entityInfo.SetValue(DateTime.Now);
                        }
                    }
                    else if (entityInfo.OperationType == DataFilterType.UpdateByObject) // 更新操作
                    {
                        if (entityInfo.PropertyName == nameof(EntityBase.UpdateTime))
                        {
                            entityInfo.SetValue(DateTime.Now);
                        }
                    }

                    // 超管时排除各种过滤器

                    // 配置实体软删除过滤器
                    db.QueryFilter.AddTableFilter<IDeletedFilter>(u => u.IsDelete == false);
                };
            });
        });
    }
}
