namespace ThingsEdge.Common.Utils;

public sealed class Signal : Exception, IConstant, IComparable, IComparable<Signal>
{
    public int Id => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }

    public int CompareTo(Signal? other)
    {
        throw new NotImplementedException();
    }
}
