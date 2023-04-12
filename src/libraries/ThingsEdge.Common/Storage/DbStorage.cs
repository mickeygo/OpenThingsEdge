using LiteDB;

namespace ThingsEdge.Common.Storage;

public sealed class DbStorage : LiteRepository, IDbStorage
{
    public DbStorage(string connectionString) : base(connectionString)
    {

    }
}