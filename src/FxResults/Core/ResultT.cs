using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResults.Core;

/// <summary>
/// A wrapper type representing either a successful result or a failure with optional metadata.
/// Provides factory methods for construction and safe execution.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
/// <example>
/// var result = Result&lt;string&gt;.Success("OK");
/// var failed = Result&lt;string&gt;.Fail("Something went wrong");
/// var wrapped = Result&lt;string&gt;.Try(() => LoadFile("file.txt"));
/// </example>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;
    private readonly MetaInfo? _meta;

    /// <summary>
    /// True if the result represents success.
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// True if the result represents failure.
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// The error associated with a failed result; null if successful.
    /// </summary>
    public Error? Error => _isSuccess ? null : _error;

    /// <summary>
    /// The metadata associated with the result, if any.
    /// </summary>
    public MetaInfo? Meta => _meta;

    /// <summary>
    /// The value of the successful result.
    /// Throws <see cref="InvalidOperationException"/> if the result is a failure.
    /// </summary>
    public T Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException("No value for failed result.");

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public Result(T value, MetaInfo? meta = null)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
        _meta = meta;
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public Result(Error error, MetaInfo? meta = null)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
        _meta = meta;
    }

    /// <summary>
    /// Safely attempts to extract the value if successful.
    /// </summary>
    public bool TryGetValue(out T? value)
    {
        value = _isSuccess ? _value : default;
        return _isSuccess;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    /// <example>
    /// var result = Result&lt;int&gt;.Success(42);
    /// </example>
    public static Result<T> Success(T value, MetaInfo? meta = null) => new(value, meta);

    /// <summary>
    /// Creates a failure result from an <see cref="Error"/>.
    /// </summary>
    /// <example>
    /// var result = Result&lt;string&gt;.Fail(new Error("Invalid input", "VALIDATION"));
    /// </example>
    public static Result<T> Fail(Error error, MetaInfo? meta = null) => new(error, meta);

    /// <summary>
    /// Creates a failure result from a message.
    /// </summary>
    /// <example>
    /// var result = Result&lt;string&gt;.Fail("Something went wrong");
    /// </example>
    public static Result<T> Fail(string message) => new(new Error(message));

    /// <summary>
    /// Creates a failure result from an exception.
    /// </summary>
    /// <example>
    /// var result = Result&lt;string&gt;.Fail(ex);
    /// </example>
    public static Result<T> Fail(Exception ex, string? code = null, string? source = null, [CallerMemberName] string? caller = null) =>
        new(new Error(ex.Message, code ?? ex.GetType().Name, source, caller, ex));

    /// <summary>
    /// Executes a function and returns a success or failure result.
    /// </summary>
    /// <example>
    /// var result = Result&lt;string&gt;.Try(() => File.ReadAllText("data.txt"));
    /// </example>
    public static Result<T> Try(Func<T> func, string? source = null, [CallerMemberName] string? caller = null)
    {
        try { return func(); }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.GetType().Name, source, caller, ex);
        }
    }

    /// <summary>
    /// Executes an async function and returns a success or failure result.
    /// </summary>
    /// <example>
    /// var result = await Result&lt;User&gt;.TryAsync(() => LoadUserAsync(id));
    /// </example>
    public static async Task<Result<T>> TryAsync(Func<Task<T>> func, string? source = null, [CallerMemberName] string? caller = null)
    {
        try { return await func(); }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.GetType().Name, source, caller, ex);
        }
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
