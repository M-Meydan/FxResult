using FxResult.Core;
using FxResult.Core.Meta;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions.Helpers;

/// <summary>Pagination helpers that return Result{List{T}} with PaginationInfo in Meta.</summary>
[ExcludeFromCodeCoverage]
public static partial class ResultPaginationExtensions
{
    /// <summary>Pages an IQueryable (SQL Skip/Take). Example: <c>db.Users.ToPagedResult(page: 1, pageSize: 10)</c></summary>
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

    /// <summary>Pages an IEnumerable (in-memory). Example: <c>list.ToPagedResult(page: 1, pageSize: 10)</c></summary>
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
