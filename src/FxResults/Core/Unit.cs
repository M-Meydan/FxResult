using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace FxResults.Core;

/// <summary>
/// Represents a void result for use with<c>Result&lt; T&gt;</c>, enabling uniform handling of operations with or without return values.
/// </summary
/// <example>
/// <code>
/// public Result<Unit> SaveFile()
/// {
///     if (!File.Exists("data.txt"))
///         return Result<Unit>.Fail("File not found");
///
///     // Do some operation
///     return Result<Unit>.Success(Unit.Value);
/// }
///
/// var result = SaveFile()
///     .OnFailure(error => Log(error.Message))
///     .Then(_ => SendNotification());
/// </code>
/// </example>
public readonly struct Unit
{
    /// <summary>
    /// The singleton value of the <see cref="Unit"/> type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <inheritdoc />
    public override string ToString() => "()";
}
