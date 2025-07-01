# FxResult

FxResult is a fluent, exception-safe result type for .NET that simplifies success/failure handling, enables null-safe logic, and supports clean sync/async pipelines.

- ✅ `Result<T>` and `Result<Unit>` for consistent API responses
- 🚫 Avoids exceptions in business logic — use `.Try()`, `.ThenTry()`, `.Ensure()`, `.FailIf()`
- ✅ `.FailIfNull()` and `.FailIfNullAsync()` for safe null propagation from T? and Task<T?>
- ✅ Nullable-safe handling for `Result<T?>`, `Task<T?>`, and `Task<Result<T?>>`
- 🧪 Includes test-friendly Tap(out var), metadata flow, and Try-safe result creation
- 🔄 Full sync/async support with `.OnSuccess()`, `.OnFailure()`, `.OnFinally()`
- 📦 Metadata, pagination, and error modelling included
- 🌐 GitHub: [FxResults Repository](https://github.com/M-Meydan/FxResult)

Install:
```csharp
dotnet add package FxResult
```
---

### 🧩 How to Use It
```csharp
using FxResult;
using R = FxResult.Result; // Alias for cleaner chaining

var result = await R<string>
    .Try(() => CaptureUserInput("Hello"))                              // R1: get input or capture thrown exception
    .Ensure(x => !string.IsNullOrWhiteSpace(x), "EMPTY", "Input is empty") // R2: validate
    .FailIf(x => x.Length < 3, "SHORT", "Too short")                   // R3: fail early on condition
    .ThenAsync(x => TryFindInCacheAsync(x))                            // R4: nullable Task<string?>
    .FailIfNullAsync("NOT_FOUND", "Value not found")                   // R5: null-safe async unwrap
    .Tap(out var original)                                             // R6: capture for rollback/log
    .Then(SaveToDatabase)                                              // R7: simulate saving to DB
    .OnFailure(res =>
    {
        LogError(res.Error);
        RollbackTransaction(original.Value);
        return R<string>.Success("fallback");                          // R8: fallback value
    })
    .OnSuccess(res => CommitTransaction(original.Value))               // R9: commit transaction
    .OnFinally(_ => Console.WriteLine($"Flow complete for: {original.Value}")); // R10: always log
```
 ---
 
### 🔁 Flow Overview
Each step returns a Result<T> (R1 → R2 → ...). If a step succeeds, the chain continues and evaluates the next operation. If a step fails, execution short-circuits and the failure is passed through to the end of the chain — skipping intermediate steps, but still triggering any registered OnFailure and OnFinally hooks.

For source, docs, and advanced usage, visit: 👉 https://github.com/M-Meydan/FxResult

