using SqlSugar;

namespace ThingsEdge.Application.Infrastructure;

/// <summary>
/// 分页拓展类
/// </summary>
public static class SqlSugarPagedExtensions
{
    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="queryable">Sugar Queryable</param>
    /// <param name="pageIndex">pageIndex 是从1开始</param>
    /// <param name="pageSize">页容量</param>
    /// <returns></returns>
    public static PagedList<TEntity> ToPagedList<TEntity>(this ISugarQueryable<TEntity> queryable, int pageIndex, int pageSize)
        where TEntity : new()
    {
        int total = 0, totalPage = 0;
        var items = queryable.ToPageList(pageIndex, pageSize, ref total, ref totalPage);
        return new PagedList<TEntity>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = total,
            TotalPages = totalPage,
            HasNextPage = pageIndex < totalPage,
            HasPrevPage = pageIndex - 1 > 0
        };
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="queryable">Sugar Queryable</param>
    /// <param name="pageIndex">pageIndex 是从1开始</param>
    /// <param name="pageSize">页容量</param>
    /// <returns></returns>
    public static async Task<PagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> queryable, int pageIndex, int pageSize)
        where TEntity : new()
    {
        RefAsync<int> total = 0, totalPage = 0;
        var items = await queryable.ToPageListAsync(pageIndex, pageSize, total, totalPage);
        return new PagedList<TEntity>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Items = items,
            TotalCount = total,
            TotalPages = totalPage,
            HasNextPage = pageIndex < totalPage,
            HasPrevPage = pageIndex - 1 > 0
        };
    }
}
