using FxResults.Core;
using System;
using System.Runtime.CompilerServices;

namespace FxResults.ResultExtensions;

/// <summary>
/// Synchronous exception-safe transformations for Result{T}.
/// <para>
/// Examples:
/// <code>
/// var r2 = r1.ThenTry(x => ParseResult(x));
/// var r3 = r1.ThenTry(x => DangerousOp(x));
/// </code>
/// </para>
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Try/catch safe Result-returning transformation.
    /// <example>
    /// var r2 = r1.ThenTry(x => ParseResult(x));
    /// </example>
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return result.Error!.WithContext(source, caller);

        try { return transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? (transform.Method?.DeclaringType?.FullName + "." + transform.Method?.Name);
            return new Error(ex.Message, ex.GetType().Name, effectiveSource, caller, ex);
        }
    }

    /// <summary>
    /// Try/catch safe transformation.
    /// <example>
    /// var r2 = r1.ThenTry(x => DangerousOp(x));
    /// </example>
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return result.Error!.WithContext(source,caller);

        try{ return transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? (transform.Method?.DeclaringType?.FullName + "." + transform.Method?.Name);
            return new Error(ex.Message,ex.GetType().Name, effectiveSource, caller,ex);
        }
    }

    /// <summary>
    /// Exception-safe transform with result capture. Returns transformed result and sets out param to that result.
    /// <example>
    /// var r2 = r1.ThenTry(x => Dangerous(x), out var captured);
    /// </example>
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform,out Result<TOut> capturedResult, string? source = null, [CallerMemberName] string? caller = null)
    {
        capturedResult = result.ThenTry(transform, source, caller);
        return capturedResult;
    }

    /// <summary>
    /// Synchronous exception-safe transformation for Result<Unit>.
    /// </summary>
    /// <example>
    /// var r2 = r1.ThenTry(() => SomeAction());
    /// </example>
    public static Result<Unit> ThenTry(this Result<Unit> result, Action action, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return result;

        try
        {
            action();
            return Result<Unit>.Success(Unit.Value, result.Meta);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(ex, ex.GetType().Name, source, caller);
        }
    }

}