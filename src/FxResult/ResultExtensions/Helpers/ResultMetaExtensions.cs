using System.Diagnostics.CodeAnalysis;
using FxResult.Core;
using FxResult.Core.Meta;

namespace FxResult.ResultExtensions.Helpers;

/// <summary>Metadata helpers for <see cref="Result{T}"/>.</summary>
[ExcludeFromCodeCoverage]
public static class ResultMetaExtensions
{
    /// <summary>Replaces the entire MetaInfo. Example: <c>result.WithMeta(new MetaInfo(correlationId: "abc"))</c></summary>
    public static Result<T> WithMeta<T>(this Result<T> result, MetaInfo meta)
    {
        return result.IsSuccess
            ? Result<T>.Success(result.Value, meta)
            : Result<T>.Fail(result.Error, meta);
    }

    /// <summary>Adds a key to Additional metadata. Example: <c>result.WithMetaData("region", "EU")</c></summary>
    public static Result<T> WithMetaData<T>(this Result<T> result, string key, object? value)
    {
        var meta = result.Meta;
        meta = meta.WithAdditional(key, value);

        return result.WithMeta(meta);
    }

    /// <summary>Merges a dictionary into Additional metadata. Example: <c>result.WithMetaData(dict)</c></summary>
    public static Result<T> WithMetaData<T>(this Result<T> result, IDictionary<string, object?> values)
    {
        var meta = result.Meta;
        meta = meta.WithAdditional(values);

        return result.WithMeta(meta);
    }
}

