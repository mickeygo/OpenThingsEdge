namespace ThingsEdge.Application.Domain.Entities;

/// <summary>
/// 表示为实体对象
/// </summary>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Id。
    /// </summary>
    public long Id { get; set; }
}

/// <summary>
/// 表示为实体对象
/// </summary>
public interface IEntity
{

}