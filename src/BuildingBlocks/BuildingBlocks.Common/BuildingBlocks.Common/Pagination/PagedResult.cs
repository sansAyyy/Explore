namespace BuildingBlocks.Common.Pagination;

public sealed class PagedResult<T>
{
    public PagedResult(long totalCount, IEnumerable<T> items, int pageIndex = 1, int pageSize = 20)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    public PagedResult(IEnumerable<T> items, long totalCount, int pageIndex = 1, int pageSize = 20)
        : this(totalCount, items, pageIndex, pageSize)
    {
    }

    public IReadOnlyCollection<T> Items { get; }

    public long TotalCount { get; }

    public int PageIndex { get; }

    public int PageNumber => PageIndex;

    public int PageSize { get; }
}

