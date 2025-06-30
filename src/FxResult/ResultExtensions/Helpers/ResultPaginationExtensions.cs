using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;
/// <summary>
/// Extension methods for projecting paginated data into a <see cref="Result{T}"/> with pagination metadata.
/// </summary>
public static partial class ResultPaginationExtensions
{
    /// <summary>
    /// Projects a paginated <see cref="IQueryable{T}"/> into a <see cref="Result{List{T}}"/> with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A <see cref="Result{List{T}}"/> with pagination metadata in <see cref="MetaInfo"/>.</returns>
    /// <example>
    /// var result = db.Users.Where(x => x.Active).ToPagedResult(1, 10);
    /// </example>
    public static Result<List<T>> ToPagedResult<T>(this IQueryable<T> query, int page, int pageSize, [CallerMemberName] string? caller = null)
    {
        // Normalize input parameters to minimum valid values
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        try
        {
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var meta = new MetaInfo
            {
                Pagination = new PaginationInfo(page, pageSize, total)
            };

            return Result<List<T>>.Success(items, meta);
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, "DATA_ACCESS_ERROR", "ResultPaginationExtensions.ToPagedResult(IQueryable)", caller, ex);
        }
    }

    /// <summary>
    /// Projects a paginated <see cref="IEnumerable{T}"/> into a <see cref="Result{List{T}}"/> with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A <see cref="Result{List{T}}"/> with pagination metadata in <see cref="MetaInfo"/>.</returns>
    /// <example>
    /// var result = list.ToPagedResult(2, 5);
    /// </example>
    public static Result<List<T>> ToPagedResult<T>(this IEnumerable<T> source, int page, int pageSize, [CallerMemberName] string? caller = null)
    {
        // Normalize input parameters to minimum valid values
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        try
        {
            var list = source.ToList();
            var total = list.Count;
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var meta = new MetaInfo
            {
                Pagination = new PaginationInfo(page, pageSize, total)
            };

            return Result<List<T>>.Success(items, meta);
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, "DATA_ACCESS_ERROR", "ResultPaginationExtensions.ToPagedResult(IEnumerable)", caller, ex);
        }
    }
}
