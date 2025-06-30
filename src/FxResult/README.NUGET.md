# FxResult

FxResult is a fluent, exception-safe result type for .NET that simplifies success/failure handling and enables clean pipelines.

- ✅ `Result<T>` and `Result<Unit>` for consistent API responses
- 🚫 Avoids exceptions in business logic — use `.Try()`, `.ThenTry()`, `.Ensure()`, `.FailIf()`
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

var result = R<string>     // R0: define Result<T> chain
    .Try(() => CaptureUserInput("Hello"))                      // R1: get input or capture thrown exception
    .Ensure(x => !string.IsNullOrWhiteSpace(x), "EMPTY", "Input is empty") // R2: validate
    .FailIf(x => x.Length < 3, "SHORT", "Too short")            // R3: fail early on condition
    .Then(x => x.Trim().ToUpper())                              // R4: transform to uppercase
    .Tap(out var original)                                      // R5: capture result value
    .Then(SaveToDatabase)                                       // R6: simulate saving to DB
    .OnFailure(res =>                                           // R7: rollback and log on failure
    {
        LogError(res.Error);
        RollbackTransaction(original.Value);
        return R<string>.Success("default");                    // recover with fallback
    })
    .OnSuccess(res => CommitTransaction(original.Value))        // R8: commit if successful
    .OnFinally(_ => Console.WriteLine($"Flow complete for: {original.Value}")); // R9: always log original

 Ouput: Success flow
  Transaction committed for HELLO
  Flow complete for: HELLO
 
 Output: Fail flow
 Error: Input is empty
  Rollback: HELLO
  Flow complete for: HELLO
```
 ---
 
### 🔁 Flow Overview
Each step returns a Result<T> (R1 → R2 → ...). If a step succeeds, the chain continues and evaluates the next operation. If a step fails, execution short-circuits and the failure is passed through to the end of the chain — skipping intermediate steps, but still triggering any registered OnFailure and OnFinally hooks.

For source, docs, and advanced usage, visit: 👉 https://github.com/M-Meydan/FxResult
