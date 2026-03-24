namespace AspireApp.BedRock.SonetOps.DapperORM.Repository;

public class MultiResultSet
{
    private readonly List<object> _results = new();

    internal void AddResult<T>(IEnumerable<T> result)
    {
        _results.Add(result);
    }

    public IEnumerable<T> GetResult<T>(int index = 0)
    {
        if (index < 0 || index >= _results.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Result set index is out of range");

        if (_results[index] is not IEnumerable<T> result)
            throw new InvalidCastException($"Cannot cast result set at index {index} to IEnumerable<{typeof(T).Name}>");

        return result;
    }

    public T GetFirstOrDefault<T>(int index = 0)
    {
        return GetResult<T>(index).FirstOrDefault();
    }

    public int ResultCount => _results.Count;
}