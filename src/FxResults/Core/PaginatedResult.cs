namespace FxResults.Core;
/// <summary>
/// Optional paginated result wrapper for list-based APIs.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Initializes a new instance of <see cref="PaginatedResult{T}"/>.
    /// </summary>
    /// <param name="items">The list of items on the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <example>
    /// var pageResult = new PaginatedResult&lt;string&gt;(items, totalCount, page, pageSize);
    /// </example>
    public PaginatedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items.ToList().AsReadOnly();
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}