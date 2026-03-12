using FxResult.Core;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Extensions for result-based failure conditions (FailIf, FailIfNull, etc.).
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class FailIfExtensions
{
    /// <summary>
    /// Converts a nullable reference value to a <see cref="Result{T}"/> or fails if <c>null</c>.
    /// </summary>
    public static Result<T> FailIfNull<T>(
        this T? value,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : class
        => value == null
            ? new Error(code, message, source, caller)
            : value;

    /// <summary>
    /// Converts a nullable value type to a <see cref="Result{T}"/> or fails if <c>null</c>.
    /// </summary>
    public static Result<T> FailIfNull<T>(
        this T? value,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : struct
        => value == null
            ? new Error(code, message, source, caller)
            : value.Value;

    /// <summary>
    /// Converts a successful <see cref="Result{T}"/> containing a nullable value type into a non-nullable
    /// <see cref="Result{T}"/>, failing when the contained value is <c>null</c>.
    /// </summary>
    public static Result<T> FailIfNull<T>(
        this Result<T?> result,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : struct
    {
        if (!result.IsSuccess) return Result<T>.Fail(result.Error, result.Meta);

        return result.Value is null
            ? Result<T>.Fail(new Error(code, message, source, caller), result.Meta)
            : Result<T>.Success(result.Value.Value, result.Meta);
    }

    /// <summary>
    /// Converts a successful <see cref="Result{T}"/> containing a nullable reference value into a non-nullable
    /// <see cref="Result{T}"/>, failing when the contained value is <c>null</c>.
    /// </summary>
    public static Result<T> FailIfNull<T>(
        this Result<T?> result,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : class
    {
        if (!result.IsSuccess) return Result<T>.Fail(result.Error, result.Meta);

        return result.Value is null
            ? Result<T>.Fail(new Error(code, message, source, caller), result.Meta)
            : Result<T>.Success(result.Value, result.Meta);
    }

    /// <summary>
    /// Fails if the predicate evaluates to <c>true</c> for a successful result value.
    /// </summary>
    public static Result<T> FailIf<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<T, Error> errorFactory,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;

        var value = result.Value!;
        if (!predicate(value)) return result;

        var err = errorFactory(value);
        err = err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
        return Result<T>.Fail(err, result.Meta);
    }

    /// <summary>
    /// Fails if the condition is <c>true</c> for a successful result.
    /// </summary>
    public static Result<T> FailIf<T>(
        this Result<T> result,
        bool condition,
        Func<Error> errorFactory,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess || !condition) return result;

        var err = errorFactory();
        err = err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
        return Result<T>.Fail(err, result.Meta);
    }

    /// <summary>
    /// Inline version of FailIf with custom code/message.
    /// </summary>
    public static Result<T> FailIf<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => result.FailIf(predicate, val => new Error(code, message, source, caller), source, caller);

    /// <summary>
    /// Inline version of FailIf with custom code/message.
    /// </summary>
    public static Result<T> FailIf<T>(
        this Result<T> result,
        bool condition,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => result.FailIf(condition, () => new Error(code, message, source, caller), source, caller);

    /// <summary>
    /// Ensures the condition is <c>true</c> for a successful result; otherwise fails with the provided error.
    /// Intended for enforcing business invariants or non-negotiable rules.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Func<Error> errorFactory,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess || predicate(result.Value!)) return result;

        var err = errorFactory();
        return Result<T>.Fail(err with { Caller = err.Caller ?? caller, Source = err.Source ?? source }, result.Meta);
    }

    /// <summary>
    /// Ensures the condition is <c>true</c> for a successful result; otherwise fails with an error message and code.
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => result.Ensure(predicate, () => new Error(code, message, source, caller), source, caller);
}
