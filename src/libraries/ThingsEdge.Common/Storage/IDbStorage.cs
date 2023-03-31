namespace ThingsEdge.Common.Storage;

public interface IDbStorage
{
    List<T> GetAll<T>();

    List<T> GetAll<T>(Expression<Func<T, bool>> expression);

    T? GetById<T>(int id);

    T? Get<T>(Expression<Func<T, bool>> expression);

    int Insert<T>(T entity);

    bool Update<T>(T entity);

    bool Delete<T>(int id);
}
