using System.Text.Json.Serialization;

namespace FxResult.Core;

/// <summary>
/// Represents a domain error with code, message, source, and optional exception context.
/// Can be inherited to create custom error types.
/// </summary>
/// <example>
/// <code>
/// // Standard usage
/// var error = new Error("FILE_MISSING", "File not found");
/// 
/// // Custom error with additional data
/// public sealed class ValidationError : Error {
///     public string FieldName { get; }
///     public ValidationError(string field, string message) 
///         : base("VALIDATION_ERROR", message, Source: $"Field: {field}") 
///         => FieldName = field;
/// }
/// </code>
/// </example>
public record Error(
    string Code,
    string Message,
    string? Source = null,
    string? Caller = null,
    [property: JsonIgnore] Exception? Exception = null,
    [property: JsonIgnore] string? FilePath = null,
    [property: JsonIgnore] int? LineNumber = null)
{
    /// <summary>
    /// True when this error was caused by an exception. Used by logging extensions
    /// to decide between error-level (with stack trace) and warning-level logging.
    /// </summary>
    public bool HasException => Exception is not null;

    /// <summary>
    /// Lightweight origin indicator: "AccountServiceClient.cs:87 → GetAccountAsync".
    /// Replaces the need for full stack traces in structured logging.
    /// Not serialized — internal diagnostics only.
    /// </summary>
    [JsonIgnore]
    public string? Location =>
        (FilePath, LineNumber, Caller) switch
        {
            (not null, not null, not null) => $"{Path.GetFileName(FilePath)}:{LineNumber} → {Caller}",
            (not null, not null, null) => $"{Path.GetFileName(FilePath)}:{LineNumber}",
            (null, null, not null) => Caller,
            _ => null
        };

    /// <summary>
    /// Safe fallback used when a <see cref="Result{T}"/> instance is in its default/uninitialized state.
    /// Keeping a non-null error here prevents null-reference exceptions when failure flows propagate.
    /// </summary>
    public static Error Uninitialized { get; } = new(
        "uninitialized_result",
        "Result was in an uninitialized (default) state.");

    /// <summary>
    /// Sets or overrides the error's context fields.
    /// </summary>
    public Error WithContext(string? source = null, string? caller = null)
        => this with
        {
            Source = source ?? Source,
            Caller = caller ?? Caller,
        };

    /// <summary>
    /// Implicitly converts a string code to an Error, using the code as the message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <example>
    /// Error error = "SOMETHING_WENT_WRONG";
    /// </example>
    public static implicit operator Error(string code) => new(code, code);

    /// <summary>
    /// Creates an Error from a (code, message) tuple.
    /// </summary>
    public static implicit operator Error((string code, string? message) errorCode)
    {
        var message = string.IsNullOrWhiteSpace(errorCode.message)
                      ? errorCode.code
                      : errorCode.message;

        return new Error(errorCode.code, message);
    }
}

