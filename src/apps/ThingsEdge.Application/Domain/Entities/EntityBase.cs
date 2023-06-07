namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 表示实现的对象为实体。
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 只包含Id的实体基类。
/// </summary>
public abstract class EntityBaseId : IEntity
{
    /// <summary>
    /// 主键。
    /// </summary>
    /// <remarks>为了方便迁移，主键不设置为自增增长。</remarks>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
    public long Id { get; set; }

    /// <summary>
    /// 实体是否为临时创建的。
    /// </summary>
    /// <returns></returns>
    public bool IsTransient()
    {
        return Id <= 0;
    }
}

/// <summary>
/// 带审计的实体基类。
/// </summary>
public abstract class EntityBase : EntityBaseId, IDeletedFilter
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除")]
    public bool IsDelete { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true)]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true)]
    public DateTime UpdateTime { get; set; }
}
