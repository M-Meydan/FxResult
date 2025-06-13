namespace FxResults.Core;

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
    string? Caller = null,
    string? Source = null,
    Exception? Exception = null)
{
    /// <summary>
    /// Adds caller and optional source context to the error.
    /// </summary>
    public Error WithContext(string caller, string? source = null) =>
        this with
        {
            Caller = Caller ?? caller,
            Source = Source ?? source ?? caller
        };


    // <summary>
    /// Creates a new <see cref="Error"/> instance.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="caller">The caller where the error was handled.</param>
    /// <param name="source">The origin of the error.</param>
    /// <param name="ex">An optional exception associated with the error.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    /// <example>
    /// var error = Error.Create("Failed", caller: nameof(MyMethod), source: "DB.Save", ex: exception);
    /// </example>
    public static Error Create(string message, string code = "error", string? caller = null, string? source = null, Exception? ex = null) =>
        new(message, code, caller, source, ex);

    /// <summary>
    /// Implicitly converts a string message to an Error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <example>
    /// Error error = "Something went wrong";
    /// </example>
    public static implicit operator Error(string message) => new(message);
}

