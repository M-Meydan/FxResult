namespace FxResult.Core;

/// <summary>
/// Represents a domain error with message, code, source, and optional exception context.
/// Can be inherited to create custom error types.
/// </summary>
/// <example>
/// <code>
/// // Standard usage
/// var error = new Error("File not found", "FILE_MISSING");
/// 
/// // Custom error with additional data
/// public sealed class ValidationError : Error {
///     public string FieldName { get; }
///     public ValidationError(string field, string message) 
///         : base(message, $"VALIDATION_ERROR", $"Field: {field}") 
///         => FieldName = field;
/// }
/// </code>
/// </example>
public record Error(
    string Message,
    string Code = "error",
    string? Source = null,
    string? Caller = null,
    Exception? Exception = null)
{
    /// Sets or overrides the error's Source and Caller.
    /// <param name="source">Component or module name</param>
    /// <param name="caller">Method name where the error is being enriched</param>
    public Error WithContext(string? source=null, string? caller = null) => this with { Source = source, Caller = caller };


    /// <summary>
    /// Implicitly converts a string message to an Error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <example>
    /// Error error = "Something went wrong";
    /// </example>
    public static implicit operator Error(string message) => new(message);
}

