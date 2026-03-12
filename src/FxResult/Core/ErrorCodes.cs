using System.Diagnostics.CodeAnalysis;

namespace FxResult.Core;

/// <summary>
/// Canonical error-code constants used by the Result pattern.
/// Replaces resource-file lookups so the library stays dependency-free.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ErrorCodes
{
    public const string UNHANDLED_ERROR = "UNHANDLED_ERROR";
    public const string NULL_VALUE = "NULL_VALUE";
    public const string DATA_ACCESS_ERROR = "DATA_ACCESS_ERROR";
    public const string ON_SUCCESS_ERROR = "ON_SUCCESS_ERROR";
    public const string ON_SUCCESS_ASYNC_ERROR = "ON_SUCCESS_ASYNC_ERROR";
}
