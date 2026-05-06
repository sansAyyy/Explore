namespace BuildingBlocks.Common.Pagination;

public class PagedRequest
{
    public int PageIndex { get; set; } = 1;

    public int PageNumber
    {
        get => PageIndex;
        set => PageIndex = value;
    }

    public int PageSize { get; set; } = 20;
}

