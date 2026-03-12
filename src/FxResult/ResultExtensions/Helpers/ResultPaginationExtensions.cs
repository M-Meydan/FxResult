using FxResult.Core;
using FxResult.Core.Meta;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions.Helpers;

/// <summary>
/// Extension methods for projecting paginated data into a <see cref="Result{T}"/> with pagination metadata.
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class ResultPaginationExtensions
{
    /// <summary>
    /// Projects a paginated <see cref="IQueryable{T}"/> into a <see cref="Result{T}"/> containing a <see cref="List{T}"/>
    /// and pagination metadata stored in <see cref="MetaInfo"/>.
    /// </summary>
    public static Result<List<T>> ToPagedResult<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        [CallerMemberName] string? caller = null)
    {
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
            return Errors.DataAccess(ex, source: "ResultPaginationExtensions.ToPagedResult(IQueryable)", caller: caller);
        }
    }

    /// <summary>
    /// Projects a paginated <see cref="IEnumerable{T}"/> into a <see cref="Result{T}"/> containing a <see cref="List{T}"/>
    /// and pagination metadata stored in <see cref="MetaInfo"/>.
    /// </summary>
    public static Result<List<T>> ToPagedResult<T>(
        this IEnumerable<T> source,
        int page,
        int pageSize,
        [CallerMemberName] string? caller = null)
    {
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
            return Errors.DataAccess(ex, source: "ResultPaginationExtensions.ToPagedResult(IEnumerable)", caller: caller);
        }
    }
}
