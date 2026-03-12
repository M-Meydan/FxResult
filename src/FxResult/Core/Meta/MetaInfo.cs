using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FxResult.Core.Meta;

/// <summary>
/// Carries metadata alongside a <see cref="Result{T}"/>.
/// <list type="bullet">
///   <item><see cref="Additional"/> — public metadata safe for API responses (ProblemDetails "metadata").</item>
///   <item><see cref="Trace"/> — internal diagnostics for structured logging / AppInsights only. Never sent to the client.</item>
/// </list>
/// </summary>
[ExcludeFromCodeCoverage]
public readonly record struct MetaInfo
{
    /// <summary>
    /// Correlation identifier for tracing across services.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the pagination information for the current data set.
    /// </summary>
    public PaginationInfo? Pagination { get; init; }

    /// <summary>
    /// Public metadata included in API responses (ProblemDetails "metadata" extension).
    /// Use for business identifiers the client is expected to consume.
    /// </summary>
    public ImmutableDictionary<string, object?> Additional { get; init; }

    /// <summary>
    /// Internal trace/diagnostics data for structured logging and AppInsights only.
    /// Never exposed to the client. Use for caller info, remote URLs, status codes,
    /// timing, feature context — anything needed for troubleshooting.
    /// </summary>
    public ImmutableDictionary<string, object?> Trace { get; init; }

    /// <summary>
    /// Initializes a new instance of the MetaInfo class with default values.
    /// </summary>
    public MetaInfo()
    {
        Additional = ImmutableDictionary<string, object?>.Empty;
        Trace = ImmutableDictionary<string, object?>.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the MetaInfo class with specified values.
    /// </summary>
    public MetaInfo(
       string? correlationId = null,
       PaginationInfo? pagination = null,
       ImmutableDictionary<string, object?>? additional = null,
       ImmutableDictionary<string, object?>? trace = null)
    {
        CorrelationId = correlationId;
        Pagination = pagination;
        Additional = additional ?? ImmutableDictionary<string, object?>.Empty;
        Trace = trace ?? ImmutableDictionary<string, object?>.Empty;
    }

    /// <summary>
    /// Returns a new MetaInfo with additional trace entries merged. Existing keys are preserved.
    /// </summary>
    public MetaInfo WithTrace(params KeyValuePair<string, object?>[] entries)
    {
        var builder = Trace.ToBuilder();
        foreach (var kvp in entries)
            builder.TryAdd(kvp.Key, kvp.Value);

        return this with { Trace = builder.ToImmutable() };
    }

    /// <summary>
    /// Returns a new MetaInfo with additional trace entries merged from a dictionary.
    /// </summary>
    public MetaInfo WithTrace(IEnumerable<KeyValuePair<string, object?>> entries)
    {
        var builder = Trace.ToBuilder();
        foreach (var kvp in entries)
            builder.TryAdd(kvp.Key, kvp.Value);

        return this with { Trace = builder.ToImmutable() };
    }

    /// <summary>
    /// Returns a new MetaInfo with trace entries from tuple pairs.
    /// </summary>
    /// <example>
    /// <code>
    /// new MetaInfo().WithTrace(("correlationId", "abc"), ("statusCode", 200));
    /// </code>
    /// </example>
    public MetaInfo WithTrace(params (string Key, object? Value)[] entries)
    {
        var builder = Trace.ToBuilder();
        foreach (var (key, value) in entries)
            builder.TryAdd(key, value);

        return this with { Trace = builder.ToImmutable() };
    }

    /// <summary>
    /// Builds a dictionary suitable for logger BeginScope,
    /// merging <see cref="Trace"/> data with optional caller-provided business context.
    /// <see cref="Additional"/> is deliberately excluded — it belongs in the API response, not the log scope.
    /// </summary>
    public Dictionary<string, object?> BuildLogScope(Dictionary<string, object?>? businessContext = null)
    {
        var trace = Trace ?? ImmutableDictionary<string, object?>.Empty;
        var scope = new Dictionary<string, object?>(trace.Count + (businessContext?.Count ?? 0));

        foreach (var (key, value) in trace)
            scope[key] = value;

        if (businessContext is not null)
        {
            foreach (var (key, value) in businessContext)
                scope[key] = value;
        }

        return scope;
    }
}
