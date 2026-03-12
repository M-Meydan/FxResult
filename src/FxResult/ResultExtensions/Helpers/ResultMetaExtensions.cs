using System.Diagnostics.CodeAnalysis;
using FxResult.Core;
using FxResult.Core.Meta;

namespace FxResult.ResultExtensions.Helpers;

/// <summary>
/// Extension methods for working with metadata on <see cref="Result{T}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ResultMetaExtensions
{
    /// <summary>
    /// Replaces the metadata on the result.
    /// </summary>
    public static Result<T> WithMeta<T>(this Result<T> result, MetaInfo meta)
    {
        return result.IsSuccess
            ? Result<T>.Success(result.Value, meta)
            : Result<T>.Fail(result.Error, meta);
    }

    /// <summary>
    /// Adds or updates a single metadata entry in the result's
    /// <see cref="MetaInfo.Additional"/> dictionary.
    /// </summary>
    public static Result<T> WithMetaData<T>(this Result<T> result, string key, object? value)
    {
        var meta = result.Meta;
        meta = meta.WithAdditional(key, value);

        return result.WithMeta(meta);
    }

    /// <summary>
    /// Adds or updates multiple metadata entries in the result's
    /// <see cref="MetaInfo.Additional"/> dictionary.
    /// </summary>
    public static Result<T> WithMetaData<T>(this Result<T> result, IDictionary<string, object?> values)
    {
        var meta = result.Meta;
        meta = meta.WithAdditional(values);

        return result.WithMeta(meta);
    }
}

