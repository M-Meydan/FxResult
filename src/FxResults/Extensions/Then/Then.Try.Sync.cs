using FxResults.Core;
using System;
using System.Runtime.CompilerServices;

namespace FxResults.Extensions;

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
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        if (!result.TryGetValue(out var value))
            return result.Error!.WithContext(caller, source);

        try { return transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? transform.Method?.Name;
            return new Error(ex.Message, ex.GetType().Name, caller, effectiveSource, ex);
        }
    }

    /// <summary>
    /// Try/catch safe transformation.
    /// <example>
    /// var r2 = r1.ThenTry(x => DangerousOp(x));
    /// </example>
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        if (!result.TryGetValue(out var value))
            return result.Error!.WithContext(caller, source);

        try{ return transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? transform.Method?.Name;
            return new Error(ex.Message,ex.GetType().Name, caller, effectiveSource,ex);
        }
    }

    /// <summary>
    /// Exception-safe transform with result capture. Returns transformed result and sets out param to that result.
    /// <example>
    /// var r2 = r1.ThenTry(x => Dangerous(x), out var captured);
    /// </example>
    /// </summary>
    public static Result<TOut> ThenTry<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform,out Result<TOut> capturedResult, string? source = null, [CallerMemberName] string caller = "")
    {
        capturedResult = result.ThenTry(transform, source, caller);
        return capturedResult;
    }
}