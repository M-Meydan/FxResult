using FxResults.Core;

namespace FxResults.ResultExtensions;

/// <summary>
/// Extension methods for working with metadata on <see cref="Result{T}"/>.
/// </summary>
public static class ResultMetaExtensions
{
    /// <summary>
    /// Replaces the metadata on the result.
    /// </summary>
    /// <example>
    /// var resultWithMeta = result.WithMeta(new MetaInfo { ... });
    /// </example>
    public static Result<T> WithMeta<T>(this Result<T> result, MetaInfo meta)
    {
        return result.IsSuccess
            ? Result<T>.Success(result.Value, meta)
            : Result<T>.Fail(result.Error!, meta);
    }

    /// <summary>
    /// Adds or updates a single metadata entry in the result's MetaInfo.Additional dictionary.
    /// </summary>
    /// <example>
    /// var result = result.WithMetaData("env", "staging");
    /// </example>
    public static Result<T> WithMetaData<T>(this Result<T> result, string key, object? value)
    {
        var meta = result.Meta.HasValue ? result.Meta.Value : new MetaInfo();
        meta = meta.WithAdditional(key, value);

        return result.WithMeta(meta);
    }

    /// <summary>
    /// Adds or updates multiple metadata entries in the result's MetaInfo.Additional dictionary.
    /// </summary>
    /// <example>
    /// var result = result.WithMetaData(new Dictionary<string, object?> { ["region"] = "EU" });
    /// </example>
    public static Result<T> WithMetaData<T>(this Result<T> result, IDictionary<string, object?> values)
    {
        var meta = result.Meta.HasValue ? result.Meta.Value : new MetaInfo();
        meta = meta.WithAdditional(values);

        return result.WithMeta(meta);
    }
}

