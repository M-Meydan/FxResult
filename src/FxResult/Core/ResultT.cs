using FxResult.Core.Meta;
using System.Runtime.CompilerServices;

namespace FxResult.Core;

/// <summary>
/// Non-generic view over <see cref="Result{T}"/>.
/// Used by API filters/middleware to format responses and log failures without knowing <c>T</c>.
/// </summary>
public interface IResult
{
    /// <summary>
    /// True if the result represents success.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// True if the result represents failure.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Always non-null. On success returns a sentinel (e.g. <see cref="Error.Uninitialized"/>).
    /// </summary>
    Error Error { get; }

    /// <summary>
    /// Always non-null. Defaults to empty <see cref="MetaInfo"/> when none was provided.
    /// </summary>
    MetaInfo Meta { get; }

    /// <summary>
    /// Boxed value when successful; otherwise null. Enables non-generic handling in filters.
    /// </summary>
    object? ValueObject { get; }
}

/// <summary>
/// A wrapper type representing either a successful result or a failure with optional metadata.
///
/// Summary:
/// - Keeps the type as a <c>struct</c> for low allocation and value semantics.
/// - Implements <see cref="IResult"/> so MVC filters can handle any <see cref="Result{T}"/> without reflection.
/// - Guarantees <see cref="IResult.Error"/> and <see cref="IResult.Meta"/> are never null (safe to consume in logging/formatting).
/// - Uses a sentinel error (<see cref="Error.Uninitialized"/>) on success to satisfy the non-null contract.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
/// <example>
/// var ok = Result&lt;string&gt;.Success("OK");
/// var failed = Result&lt;string&gt;.Fail(new Error("VALIDATION_ERROR", "Invalid"));
/// var wrapped = Result&lt;string&gt;.Try(() => LoadFile("file.txt"));
/// </example>
public readonly struct Result<T> : IResult
{
    private readonly T? _value;
    private readonly Error? _error;

    /// <summary>
    /// True if the result represents success.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// True if the result represents failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error associated with a failed result; a sentinel (<see cref="Error.Uninitialized"/>) on success.
    /// This is non-null to make <see cref="IResult"/> safe to consume without null checks.
    /// </summary>
    public Error Error => IsSuccess ? Error.Uninitialized : (_error ?? Error.Uninitialized);

    /// <summary>
    /// The metadata associated with the result (never null; defaults to empty meta).
    /// </summary>
    public MetaInfo Meta { get; }

    /// <summary>
    /// The value of the successful result.
    /// Throws <see cref="InvalidOperationException"/> if the result is a failure.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value for failed result.");

    /// <summary>
    /// Boxed value for non-generic consumers (filters/middleware). Null on failure.
    /// </summary>
    public object? ValueObject => IsSuccess ? _value : null;

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public Result(T value, MetaInfo? meta = null)
    {
        _value = value;
        _error = null;
        IsSuccess = true;
        Meta = meta ?? new MetaInfo();
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public Result(Error error, MetaInfo? meta = null)
    {
        _value = default;
        _error = error ?? throw new ArgumentNullException(nameof(error));
        IsSuccess = false;
        Meta = meta ?? new MetaInfo();
    }

    /// <summary>
    /// Safely attempts to extract the value if successful.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        value = IsSuccess ? _value : default;
        return IsSuccess;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static Result<T> Success(T value, MetaInfo? meta = null) => new(value, meta);

    /// <summary>
    /// Creates a failure result from an <see cref="Error"/>.
    /// </summary>
    public static Result<T> Fail(Error error, MetaInfo? meta = null) => new(error, meta);

    /// <summary>
    /// Creates a failure result from a message (uses UNHANDLED_ERROR code).
    /// </summary>
    public static Result<T> Fail(string message) => new(new Error(ErrorCodes.UNHANDLED_ERROR, message));

    /// <summary>
    /// Creates a failure result from an exception.
    /// </summary>
    public static Result<T> Fail(
        Exception ex,
        string? code = null,
        string? source = null,
        [CallerMemberName] string? caller = null) =>
        new(new Error(code ?? ErrorCodes.UNHANDLED_ERROR, ex.Message, source, caller, ex));

    /// <summary>
    /// Executes a function and returns a success or failure result.
    /// Automatically captures caller location (file, line, method) on failure.
    /// </summary>
    public static Result<T> Try(
        Func<T> func,
        Func<Exception, Error?>? onException = null,
        string? source = null,
        [CallerMemberName] string? caller = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        try { return func(); }
        catch (Exception ex)
        {
            return BuildError(ex, onException, source, caller, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Executes an async function and returns a success or failure result.
    /// Automatically captures caller location (file, line, method) on failure.
    /// </summary>
    public static async Task<Result<T>> TryAsync(
        Func<Task<T>> func,
        Func<Exception, Error?>? onException = null,
        string? source = null,
        [CallerMemberName] string? caller = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        try { return await func().ConfigureAwait(false); }
        catch (Exception ex)
        {
            return BuildError(ex, onException, source, caller, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Executes an async function with cancellation support and returns a success or failure result.
    /// Automatically captures caller location (file, line, method) on failure.
    /// </summary>
    public static async Task<Result<T>> TryAsync(
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default,
        Func<Exception, Error?>? onException = null,
        string? source = null,
        [CallerMemberName] string? caller = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        try { return await func(cancellationToken).ConfigureAwait(false); }
        catch (Exception ex)
        {
            return BuildError(ex, onException, source, caller, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Builds an <see cref="Error"/> from a caught exception, applying the optional
    /// <paramref name="onException"/> classifier and enriching with caller location.
    /// </summary>
    private static Error BuildError(
        Exception ex,
        Func<Exception, Error?>? onException,
        string? source,
        string? caller,
        string? filePath,
        int lineNumber)
    {
        var custom = onException?.Invoke(ex);

        if (custom is not null)
        {
            return custom.WithContext(source ?? custom.Source, caller ?? custom.Caller) with
            {
                FilePath = filePath,
                LineNumber = lineNumber
            };
        }

        return new Error(
            ErrorCodes.UNHANDLED_ERROR,
            ex.Message, source, caller, ex, filePath, lineNumber);
    }

    /// <summary>
    /// Implicitly converts a value into a success result.
    /// </summary>
    public static implicit operator Result<T>(T value) => new(value);

    /// <summary>
    /// Implicitly converts an error into a failed result.
    /// </summary>
    public static implicit operator Result<T>(Error error) => new(error);
}
