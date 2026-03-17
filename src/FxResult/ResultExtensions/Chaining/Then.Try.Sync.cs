using FxResult.Core;
using FxResult.ResultExtensions.Helpers;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Sync exception-safe transforms. Catches exceptions and returns Error.</summary>
public static partial class ThenExtensions
{
    /// <summary>Exception-safe Result-returning transform. Example: <c>result.ThenTry(x =&gt; Parse(x))</c></summary>
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

    /// <summary>Exception-safe value transform. Example: <c>result.ThenTry(int.Parse)</c></summary>
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

    /// <summary>Exception-safe transform with out capture. Example: <c>result.ThenTry(Parse, out var parsed)</c></summary>
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

    /// <summary>Exception-safe action on Result{Unit}. Example: <c>unitResult.ThenTry(() =&gt; File.Delete(path))</c></summary>
    public static Result<RUnit> ThenTry(
        this Result<RUnit> result,
        Action action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return result;

        try
        {
            action();
            return Result<RUnit>.Success(RUnit.Value, result.Meta);
        }
        catch (Exception ex)
        {
            return Result<RUnit>.Fail(ex, ex.GetType().Name, source, caller)
                .WithMeta(result.Meta);
        }
    }
}