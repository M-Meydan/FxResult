using System.Diagnostics.CodeAnalysis;

namespace FxResult.Core;

/// <summary>
/// Represents a void result for use with <c>Result&lt;T&gt;</c>, enabling uniform handling
/// of operations with or without return values.
/// </summary>
/// <example>
/// <code>
/// public Result&lt;Unit&gt; SaveFile()
/// {
///     if (!File.Exists("data.txt"))
///         return Result&lt;Unit&gt;.Fail("File not found");
///
///     // Do some operation
///     return Result&lt;Unit&gt;.Success(Unit.Value);
/// }
///
/// var result = SaveFile()
///     .OnFailure(error =&gt; Log(error.Message))
///     .Then(_ =&gt; SendNotification());
/// </code>
/// </example>
/// <remarks>
/// This type is conceptually similar to <c>void</c> but can be used as a generic
/// type argument for <c>Result&lt;T&gt;</c> to enable fluent and uniform result handling.
/// </remarks>
[ExcludeFromCodeCoverage]
public readonly struct Unit
{
    /// <summary>
    /// The singleton value of the <see cref="Unit"/> type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <inheritdoc />
    public override string ToString() => "()";
}
