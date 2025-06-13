using System.Runtime.CompilerServices;

namespace FxResults.Core;
/// <summary>
/// A wrapper type representing either a successful result or an error.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    public bool IsSuccess => _isSuccess;
    public Error? Error => _isSuccess ? null : _error;
    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("No value for failed result.");

    /// <summary>
    /// Initializes a new successful <see cref="Result{T}"/> instance.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    public Result(T value)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
    }

    /// <summary>
    /// Initializes a new failed <see cref="Result{T}"/> instance.
    /// </summary>
    /// <param name="error">The error associated with the failed result.</param>
    public Result(Error error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    /// <summary>
    /// Tries to get the value of the result.
    /// </summary>
    /// <param name="value">The value if the result is successful; otherwise, null.</param>
    /// <returns>True if the result is successful; otherwise, false.</returns>
    public bool TryGetValue(out T? value)
    {
        value = _isSuccess ? _value : default;
        return _isSuccess;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
}

/// <summary>
/// Result factory methods.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="value">The successful result value.</param>
    /// <returns>A successful <see cref="Result{T}"/> instance.</returns>
    /// <example>
    /// var result = Result.Success(42);
    /// </example>
    public static Result<T> Success<T>(T value) => new(value);

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="error">The error associated with the failed result.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    /// <example>
    /// var result = Result.Fail<int>(new Error("An error occurred."));
    /// </example>
    public static Result<T> Fail<T>(Error error) => new(error);

    /// <summary>
    /// Creates a failed result with a message.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="message">The error message.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    /// <example>
    /// var result = Result.Fail<int>("An error occurred.");
    /// </example>
    public static Result<T> Fail<T>(string message) => new Error(message);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="ex">The exception that caused the failure.</param>
    /// <param name="source">An optional source of the error.</param>
    /// <param name="caller">The name of the method that caught the exception.</param>
    /// <returns>A failed <see cref="Result{T}"/> instance.</returns>
    /// <example>
    /// try
    /// {
    ///     // Some operation that may fail
    /// }
    /// catch (Exception ex)
    /// {
    ///     var result = Result.Fail<int>(ex);
    /// }
    /// </example>
    public static Result<T> Fail<T>(Exception ex, string? source = null, [CallerMemberName] string caller = "") =>
        new Error(ex.Message, ex.GetType().Name, caller, source, ex);
}