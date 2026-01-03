namespace Pollus.Collections;

public ref struct ListEnumerable<TItem>
{
    private readonly IReadOnlyList<TItem> _list;

    public ListEnumerable(IReadOnlyList<TItem> list)
    {
        _list = list;
    }

    public ListEnumerator<TItem> GetEnumerator() => new ListEnumerator<TItem>(_list);
}

public ref struct ListEnumerator<TItem>
{
    private readonly IReadOnlyList<TItem> _list;
    private int _index;

    public ListEnumerator(IReadOnlyList<TItem> list)
    {
        _list = list;
        _index = -1;
    }

    public bool MoveNext()
    {
        _index++;
        return _index < _list.Count;
    }

    public TItem Current => _list[_index];
}