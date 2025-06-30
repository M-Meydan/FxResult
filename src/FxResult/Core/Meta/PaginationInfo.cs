namespace FxResult.Core;
public class PaginationInfo
{
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; } = 0;

    /// <summary>
    /// The total number of pages, calculated from <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// </summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Indicates whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginationInfo"/> class.
    /// </summary>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    public PaginationInfo(int page = 1, int pageSize = 10, int totalCount = 0)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginationInfo"/> class with default values.
    /// </summary>
    public PaginationInfo() { }
}