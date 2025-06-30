namespace FxResult.Core;

public static class MetaInfoExtensions
{
    /// <summary>
    /// Adds or updates a key-value pair in the MetaInfo's Additional dictionary.
    /// Returns a new MetaInfo instance with the updated metadata.
    /// </summary>
    public static MetaInfo WithAdditional(this MetaInfo meta, string key, object? value)
    {
        var newAdditional = meta.Additional.SetItem(key, value);
        return meta with { Additional = newAdditional };
    }

    /// <summary>
    /// Adds multiple key-value pairs into MetaInfo's Additional dictionary.
    /// Returns a new MetaInfo instance with the updated metadata.
    /// </summary>
    public static MetaInfo WithAdditional(this MetaInfo meta, IDictionary<string, object?> values)
    {
        var newAdditional = meta.Additional.SetItems(values);
        return meta with { Additional = newAdditional };
    }
}