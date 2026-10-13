namespace Pos.BuildingBlocks.Application.Pagination;

/// <summary>A page of results plus the information a client needs to request the next one.</summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
