namespace ThingsEdge.Common.Storage;

/// <summary>
/// LiteDB 管理对象。
/// </summary>
public sealed class DbManager
{
    /// <summary>
    /// 创建的仓储对象。
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static IDbStorage Create(string connectionString = "LiteDB.db")
    {
        return new DbStorage(connectionString);
    }
}
