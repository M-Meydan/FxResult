using FxResult.Core;
using FxResult.ResultExtensions.Helpers;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Synchronous exception-safe transformations for <see cref="Result{T}"/>.
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Try/catch-safe <see cref="Result{TOut}"/>-returning transformation.
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return Result<TOut>.Fail(result.Error.WithContext(source, caller), result.Meta);

        try
        {
            return transform(value!);
        }
        catch (Exception ex)
        {
            var effectiveSource =
                source ?? $"{transform.Method?.DeclaringType?.FullName}.{transform.Method?.Name}";

            return Result<TOut>.Fail(
                new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex),
                result.Meta);
        }
    }

    /// <summary>
    /// Try/catch-safe synchronous transformation.
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return Result<TOut>.Fail(result.Error.WithContext(source, caller), result.Meta);

        try
        {
            return Result<TOut>.Success(transform(value!), result.Meta);
        }
        catch (Exception ex)
        {
            var effectiveSource =
                source ?? $"{transform.Method?.DeclaringType?.FullName}.{transform.Method?.Name}";

            return Result<TOut>.Fail(
                new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex),
                result.Meta);
        }
    }

    /// <summary>
    /// Exception-safe transformation with result capture.
    /// Returns the transformed result and assigns it to an <c>out</c> parameter.
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> transform,
        out Result<TOut> capturedResult,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        capturedResult = result.ThenTry(transform, source, caller);
        return capturedResult;
    }

    /// <summary>
    /// Synchronous exception-safe transformation for <see cref="Result{Unit}"/>.
    /// </summary>
    public static Result<Unit> ThenTry(
        this Result<Unit> result,
        Action action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return result;

        try
        {
            action();
            return Result<Unit>.Success(Unit.Value, result.Meta);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(ex, ex.GetType().Name, source, caller)
                .WithMeta(result.Meta);
        }
    }
}