using System.Diagnostics.CodeAnalysis;
using FxResult.Core.Meta;

namespace FxResult.Core;

[ExcludeFromCodeCoverage]
public static class MetaInfoExtensions
{
    /// <summary>Adds or updates a key in Additional. Example: <c>meta.WithAdditional("region", "EU")</c></summary>
    public static MetaInfo WithAdditional(this MetaInfo meta, string key, object? value)
    {
        var newAdditional = meta.Additional.SetItem(key, value);
        return meta with { Additional = newAdditional };
    }

    /// <summary>Merges a dictionary into Additional. Example: <c>meta.WithAdditional(dict)</c></summary>
    public static MetaInfo WithAdditional(this MetaInfo meta, IDictionary<string, object?> values)
    {
        var newAdditional = meta.Additional.SetItems(values);
        return meta with { Additional = newAdditional };
    }

    /// <summary>Adds tuples into Additional. Example: <c>meta.WithAdditional(("key1", "v1"), ("key2", 42))</c></summary>
    public static MetaInfo WithAdditional(this MetaInfo meta, params (string Key, object? Value)[] entries)
    {
        var builder = meta.Additional.ToBuilder();
        foreach (var (key, value) in entries)
            builder[key] = value;

        return meta with { Additional = builder.ToImmutable() };
    }
}