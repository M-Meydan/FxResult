using FxResults.Core;
using System;
using System.Runtime.CompilerServices;

namespace FxResults.Extensions.FailIf;

/// <summary>
/// Extensions for result-based failure conditions (FailIf, FailIfNull, etc.).
/// </summary>
public static class FailIfExtensions
{
    /// <summary>
    /// Converts a nullable object to a Result or fails if null.
    /// <example>
    /// var result = someNullableValue.FailIfNull("Value cannot be null.", source: "PaymentService.Method");
    /// </example>
    /// </summary>
    public static Result<T> FailIfNull<T>(this T? value, string message, string code = "NULL_VALUE", string? source = null, [CallerMemberName] string? caller = null) where T : class
        => value == null ? new Error(message, code, caller, source) : Result.Success(value);

    /// <summary>
    /// Fails if the predicate on the value is true.
    /// <example>
    /// var result = someResult.FailIf(value => value < 0, val => new Error("Value cannot be negative."), source: "PaymentService.Validate");
    /// </example>
    /// </summary>
    public static Result<T> FailIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Error> errorFactory, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;
        if (predicate(result.Value!))
        {
            var err = errorFactory(result.Value!);
            err = err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
            return err;
        }
        return result;
    }

    /// <summary>
    /// Fails if the condition is true.
    /// <example>
    /// var result = someResult.FailIf(isInvalid, () => new Error("Invalid value"), source: "InventoryService.Check");
    /// </example>
    /// </summary>
    public static Result<T> FailIf<T>(this Result<T> result, bool condition, Func<Error> errorFactory, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess || !condition) return result;
        var err = errorFactory();
        err = err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
        return err;
    }

    /// <summary>
    /// Inline version of FailIf with custom code/message/source.
    /// <example>
    /// var result = someResult.FailIf(value => value < 0, "NEGATIVE_VALUE", "Value cannot be negative.", source: "OrderService.Create");
    /// </example>
    /// </summary>
    public static Result<T> FailIf<T>(this Result<T> result, Func<T, bool> predicate, string code, string message, string? source = null, [CallerMemberName] string? caller = null)
        => result.FailIf(predicate, val => new Error(message, code, caller, source), source, caller);

    /// <summary>
    /// Inline version of FailIf with custom code/message/source.
    /// <example>
    /// var result = someResult.FailIf(isBad, "CONDITION_FAILED", "Condition failed.", source: "ShippingService.Schedule");
    /// </example>
    /// </summary>
    public static Result<T> FailIf<T>(this Result<T> result, bool condition, string code, string message, string? source = null, [CallerMemberName] string? caller = null)
        => result.FailIf(condition, () => new Error(message, code, caller, source), source, caller);
}
