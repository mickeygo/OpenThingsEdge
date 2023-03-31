using LiteDB;

namespace ThingsEdge.Common.Storage;

public sealed class LiteDbManager
{
    private readonly string _connectionString;

    public LiteDbManager() : this(Path.Combine(AppContext.BaseDirectory, "LiteDB.db"))
    {

    }

    public LiteDbManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbStorage Create()
    {
        var db = new LiteDatabase(_connectionString);
        return new LiteDbStorage(db);
    }
}
