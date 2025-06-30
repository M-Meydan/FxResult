using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Asynchronous invariant enforcement for Result{T}.
/// Fails if the async predicate returns false.
/// </summary>
public static partial class FailIfExtensions
{
    /// <summary>
    /// Asynchronously fails if the predicate returns true for the result value.
    /// <example>
    /// var result = await someResult.FailIfAsync(async x => await IsInvalidAsync(x), "INVALID", "The value is invalid");
    /// </example>
    /// </summary>
    public static async Task<Result<T>> FailIfAsync<T>(this Result<T> result,Func<T, Task<bool>> predicateAsync,string code,string message,string? source = null,[CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;
        try
        {
            if (await predicateAsync(result.Value!))
                return new Error(message, code, source, caller);
        }
        catch (Exception ex)
        { return new Error(ex.Message, ex.GetType().Name, source, caller, ex); }

        return result;
    }

    /// <summary>
    /// Asynchronously fails if the condition resolves to true, regardless of the result value.
    /// <example>
    /// var result = await someResult.FailIfAsync(async () => await ShouldFailAsync(), "CHECK_FAILED", "Validation failed.");
    /// </example>
    /// </summary>
    public static async Task<Result<T>> FailIfAsync<T>(this Result<T> result,Func<Task<bool>> conditionAsync,string code,string message,string? source = null,[CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;

        try
        {
            if (await conditionAsync())
                return new Error(message, code, source, caller);
        }
        catch (Exception ex)
        { return new Error(ex.Message, ex.GetType().Name, source, caller, ex); }

        return result;
    }


    /// <summary>
    /// Ensures an async condition is true; otherwise, fails with the provided error.
    /// </summary>
    /// <example>
    /// await result.EnsureAsync(async x => await IsUniqueAsync(x.Id), () => new Error(...));
    /// </example>
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicateAsync, Func<Error> errorFactory, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;
        try
        {
            if (await predicateAsync(result.Value!)) return result;
        }
        catch (Exception ex)
        { return new Error(ex.Message, ex.GetType().Name, source, caller, ex); }

        var err = errorFactory();
        return err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
    }

    /// <summary>
    /// Ensures an async condition is true; otherwise, fails with the provided code/message.
    /// </summary>
    /// <example>
    /// await result.EnsureAsync(async x => await ExistsInDb(x.Id), \"NOT_FOUND\", \"Item not found.\");
    /// </example>
    public static Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicateAsync, string code, string message, string? source = null, [CallerMemberName] string? caller = null)
    {
        return result.EnsureAsync(predicateAsync, () => new Error(message, code, source, caller), source, caller);
    }

}
