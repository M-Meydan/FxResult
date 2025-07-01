using FxResult.Core;
using System;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Extensions for result-based failure conditions (FailIf, FailIfNull, etc.).
/// </summary>
public static partial class FailIfExtensions
{
    /// <summary>
    /// Converts a nullable object to a Result or fails if null.
    /// <example>
    /// var result = someNullableValue.FailIfNull("Value cannot be null.", source: "PaymentService.Method");
    /// </example>
    /// </summary>
    public static Result<T> FailIfNull<T>(this T? value, string message, string code = "NULL_VALUE", string? source = null, [CallerMemberName] string? caller = null) where T : class
        => value == null ? new Error(message, code, source, caller) : value;


    /// <summary>
    /// Converts a nullable value type to a Result or fails if null.
    /// <example>
    /// var result = someNullableInt.FailIfNull("Value cannot be null.", source: "PaymentService.Method");
    /// </example>
    /// </summary>
    public static Result<T> FailIfNull<T>(this T? value, string message, string code = "NULL_VALUE", string? source = null, [CallerMemberName] string? caller = null) where T : struct
        => value == null ? new Error(message, code, source, caller) : value.Value;

    public static Result<T> FailIfNull<T>(this Result<T?> result, string message, string code = "NULL_VALUE", string? source = null, [CallerMemberName] string? caller = null) where T : struct
    {
        if (!result.IsSuccess) return result.Error!;
        return result.Value is null
            ? new Error(message, code, source, caller)
            : Result<T>.Success(result.Value.Value, result.Meta);
    }

    public static Result<T> FailIfNull<T>(this Result<T?> result, string message, string code = "NULL_VALUE", string? source = null, [CallerMemberName] string? caller = null) where T : class
    {
        if (!result.IsSuccess) return result.Error!;
        return result.Value is null
            ? new Error(message, code, source, caller)
            : Result<T>.Success(result.Value, result.Meta);
    }


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
        => result.FailIf(predicate, val => new Error(message, code, source, caller), source, caller);

    /// <summary>
    /// Inline version of FailIf with custom code/message/source.
    /// <example>
    /// var result = someResult.FailIf(isBad, "CONDITION_FAILED", "Condition failed.", source: "ShippingService.Schedule");
    /// </example>
    /// </summary>
    public static Result<T> FailIf<T>(this Result<T> result, bool condition, string code, string message, string? source = null, [CallerMemberName] string? caller = null)
        => result.FailIf(condition, () => new Error(message, code, source, caller), source, caller);


    /// <summary>
    /// Ensures the condition is true for a successful result; otherwise, fails with the provided error.
    /// Intended for enforcing business invariants or non-negotiable rules.
    /// </summary>
    /// <example>
    /// result.Ensure(x => x.IsActive, () => new Error(\"Must be active\"), source: \"UserService.Check\");
    /// </example>
    public static Result<T> Ensure<T>(this Result<T> result,Func<T, bool> predicate, Func<Error> errorFactory,string? source = null,[CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess || predicate(result.Value!)) return result;

        var err = errorFactory();
        return err with { Caller = err.Caller ?? caller, Source = err.Source ?? source };
    }

    /// <summary>
    /// Ensures the condition is true for a successful result; otherwise, fails with an error message and code.
    /// </summary>
    /// <example>
    /// result.Ensure(x => x.Quantity > 0, \"INVALID_QUANTITY\", \"Quantity must be positive\", source: \"OrderValidation\");
    /// </example>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string code, string message, string? source = null, [CallerMemberName] string? caller = null)
    {
        return result.Ensure(predicate, () => new Error(message, code, source, caller));
    }

}
