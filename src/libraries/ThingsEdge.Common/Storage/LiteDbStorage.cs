using LiteDB;

namespace ThingsEdge.Common.Storage;

internal sealed class LiteDbStorage : IDbStorage
{
    private readonly ILiteDatabase _database;

    public LiteDbStorage(ILiteDatabase database)
    {
        _database = database;
    }

    public List<T> GetAll<T>()
    {
        var col = _database.GetCollection<T>();
        return col.FindAll().ToList();
    }

    public List<T> GetAll<T>(Expression<Func<T, bool>> expression)
    {
        var col = _database.GetCollection<T>();
        return col.Find(expression).ToList();
    }

    public T? GetById<T>(int id)
    {
        var col = _database.GetCollection<T>();
        return col.FindById(id);
    }

    public T? Get<T>(Expression<Func<T, bool>> expression)
    {
        var col = _database.GetCollection<T>();
        return col.FindOne(expression);
    }

    public int Insert<T>(T entity)
    {
        var col = _database.GetCollection<T>();
        return col.Insert(entity).AsInt32;
    }

    public bool Update<T>(T entity)
    {
        var col = _database.GetCollection<T>();
        return col.Update(entity);
    }

    public bool Delete<T>(int id)
    {
        var col = _database.GetCollection<T>();
        return col.Delete(id);
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
