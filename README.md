![NuGet](https://img.shields.io/nuget/v/FxResult.svg)
![License](https://img.shields.io/github/license/M-Meydan/FxResult)
![Build](https://img.shields.io/github/actions/workflow/status/M-Meydan/FxResult/build.yml?branch=main)

# FxResult

![NuGet](...) ![License](...) ![Build](...)

**FxResult v1.0** — A result abstraction library for .NET.  
Provides fluent, safe result handling without exceptions for flow control.

---

## Table of Contents

- [Version](#version)
- [Purpose](#purpose)
- [Key Features](#key-features)
- [Result Types](#result-types)
- [Basic Usage](#basic-usage)
- [Result Properties](#result-properties)
- [Metadata Support with MetaInfo](#metadata-support-with-metainfo)
- [Pagination Support](#pagination-support)
- [API Conversion](#api-conversion)
- [Error Results](#error-results)
  - [Error Properties](#error-properties)
  - [Domain-Specific Errors: Validation](#domain-specific-errors-validation)
  - [Logging with Correlation & Context](#logging-with-correlation--context)
  - [Error Enrichment with Context](#error-enrichment-with-context)
  - [Best usage practices](#best-usage-practices)
- [Fluent Extensions](#fluent-extensions)
  - [How Result Chains Work](#how-result-chains-work)
  - [Then: Value Transformation](#then-value-transformation)
  - [Tap: Side Effects & Capture](#tap-side-effects--capture)
  - [FailIf: Guard Conditions](#failif-guard-conditions)
  - [Ensure: Business Invariants](#ensure-business-invariants)
  - [Try: Exception-Safe Factory](#try-exception-safe-factory)
  - [ThenTry: Exception-Safe Wrapping](#thentry-exception-safe-wrapping)
  - [OnSuccess, OnFailure, OnFinally: Pipeline Outcome Hooks](#onsuccess-onfailure-onfinally-pipeline-outcome-hooks)
  - [Example of Chained Extensions](#example-of-chained-extensions)
- [Ideal Use Cases](#ideal-use-cases)
- [Author](#author)
- [License](#license)

---

## Version

**Latest:** `v1.1`  
**Status:** Stable - Actively Maintained  

---

## Purpose

FxResult simplifies business logic, improves consistency, and enables safe, expressive workflows by avoiding exceptions for expected outcomes. It provides a consistent way to handle operations that can either succeed with a value or fail with a structured error.

- **Uniform Return Types**: All services, handlers, and commands return `Result<T>`, creating a consistent and composable flow.
- **Exception-Free Business Logic**: Use .Try(), .FailIf(), .Ensure(), and .OnFailure() to handle errors and business rules without try/catch, keeping flow clear and testable.
- **Seamless API & Infrastructure Integration**: Wrap any external or internal operation that can fail (DB, file, HTTP, 3rd-party) into a `Result` using `.ThenTry()` or `.FailIfNull()`.
- **Clear Separation of Success & Failure**: `Result<T>` enforces the presence of either a `Value` or an `Error`, making handling explicit and enabling fluent pipelines via `.OnSuccess()`, `.OnFailure()`, and `.OnFinally()`.

### Why this matters in practice

- Prevents null handling bugs
- Eliminates silent exceptions or partial success states
- Makes failures traceable and meaningful (via `Error`)
- Keeps method contracts readable and consistent
- Integrates naturally with logging, validation, and monitoring

---

## Key Features

FxResult offers a rich set of features designed to streamline functional error handling and data flow in .NET applications:

- **Core Types**\
	`Result<Unit>` (for void operations), `Result<T>` (for operations returning a value), and a structured `Error` object.
- **Unified Exception-Safe Factory**\
	Use Result.Try() and Result.TryAsync() to wrap logic into safe Result<T> or Result<Unit> outcomes.
- **Fluent Chaining**\
	Methods like `Then`, `ThenTry`, `Tap`, `FailIf`, and `Ensure` for building expressive, exception-safe pipelines.
- **Async-First Design**\
	Full `async/await` support for non-blocking operations.
- **Side-Effect Hooks**\
 `.OnSuccess()`, `.OnFailure()`, and `.OnFinally()` to react to pipeline outcomes without breaking the flow.
- **API-Layer DTO Conversion**\
 Seamless conversion to API-friendly DTOs (`ToResponseDto`, `ToPublicDto`).
- **Custom Error Types**\
	Support for creating domain-specific `Error` subclasses.
- **Extensible & Testable**\ 
	Designed for easy extension and unit testing.
- **Built-in Pagination**\ 
	Helpers via `.ToPagedResult()` for efficient data pagination.
- **Rich Structured Metadata**\ 
	Attach arbitrary data using `MetaInfo`.
- **Fluent Metadata Helpers**\ 
	`.WithMeta()`, `.WithMetaData()` for easy metadata manipulation.

---

## Installation

```csharp
Install-Package FxResult
```
---

## Result Types

`Result<Unit>` and `Result<T>` are the foundation of FxResult. They represent the outcome of an operation — either success or failure — without relying on exceptions. They also support implicit conversions for simplified usage.

- **`Result<Unit>`**: Used when there's no return value, just success/failure. `Unit.Value` represents the successful void result.
- **`Result<T>`**: Used when the operation returns a value (`T`) on success, or an `Error` on failure.

---

## Basic Usage

```csharp
// Creating a successful void result
var ok = Result.Success<Unit>(Unit.Value);

// Creating a failed void result
var fail = Result.Fail<Unit>("Something went wrong");

// Creating a successful result with a value
var val = Result.Success(42);

// Creating a failed result with a structured error
var err = Result.Fail<string>(new Error("Missing field", "INVALID"));

// Safe execution wrapper using Try
var loaded = Result<string>.Try(() => LoadUserFile("users.json"));
var created = await Result<Unit>.TryAsync(() => CreateInitialUsers());

// Implicit conversion from value to success result
Result<int> implicitVal = 100;

// Implicit conversion from string to error result
Result<string> implicitErr = "Operation failed";
```

---

## Result Properties

| Property                    | Type        | Description                                                              |
| --------------------------- | ----------- | ------------------------------------------------------------------------ |
| `IsSuccess`                 | `bool`      | `true` if the result represents a successful operation                   |
| `IsFailure`                 | `bool`      | `true` if the result represents a failure (`!IsSuccess`)                 |
| `Error`                     | `Error`     | The error object (`Code`, `Message`, etc.); **`null` if successful**    |
| `Value`                     | `T`         | The returned value; **throws `InvalidOperationException` if failed**     |
| `Meta`                      | `MetaInfo?` | Optional metadata such as pagination, trace context, etc.                |
| `TryGetValue(out T? value)` | `bool`      | Returns `true` if successful and sets `value`; otherwise returns `false` |

---

## Metadata Support with MetaInfo

`Result<T>` supports attaching optional structured metadata using `MetaInfo`. While `MetaInfo` itself is a mutable class, the `WithMeta()` and `WithMetaData()` extensions on `Result<T>` return a *new* `Result<T>` instance, preserving the immutability of the `Result` chain.

This is useful for:

- Adding pagination to list results
- Tagging with trace IDs, API versions, or diagnostics
- Returning rich, extensible responses from services or APIs

### Example

```csharp
var result = Result<List<string>>.Success(items, new MetaInfo
{
    Pagination = new PaginationInfo
    {
        Page = 1,
        PageSize = 20,
        TotalCount = 45
    },
    Additional = new()
    {
        ["apiVersion"] = "v2",
        ["requestedBy"] = "admin"
    }
});

result = result
    .WithMetaData("region", "EU")
    .WithMetaData("feature", "preview");

// For direct manipulation of MetaInfo (use with caution if MetaInfo is shared):
// result.Meta.WithAdditional("customKey", "customValue");
```

---

## Pagination Support

FxResult provides built-in pagination helpers that return paged data wrapped in a `Result<List<T>>` with pagination metadata attached to `.Meta`.

### Example

```csharp
// Efficient for database queries (translates to SQL Skip/Take)
var resultFromQueryable = db.Users
    .Where(u => u.IsActive)
    .ToPagedResult(page: 2, pageSize: 10)
    .WithMetaData("requestedBy", "admin");

// Suitable for in-memory collections (iterates entire collection to count)
var resultFromEnumerable = someList
    .ToPagedResult(page: 1, pageSize: 5);

// Note: In real-world scenarios, consider wrapping ToPagedResult calls in a ThenTry/ThenTryAsync
// to handle potential exceptions from the underlying data source (e.g., database connection issues).
```

### PaginationInfo Fields

- `Page`, `PageSize`, `TotalCount`
- `TotalPages` (computed)
- `HasNextPage`, `HasPreviousPage`

---

## API Conversion

Use `ToResponseDto()` to map a `Result<T>` into a serializable DTO including metadata. This conversion leverages `ResultResponse<T>`, `PublicErrorItem`, and `PublicErrorResponse` to provide a consistent and safe API contract, separating internal error details from public ones.

```csharp
var response = result.ToResponseDto();
```

### Example Response

```json
{
  "data": [...],
  "meta": {
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalCount": 45,
      "totalPages": 5
    },
    "additional": {
      "apiVersion": "v1.0"
    }
  }
}
```

### Public Error Response Example

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "code": "FIELD_REQUIRED",
        "message": "Email is required.",
        "source": "User.Email"
      },
      {
        "code": "FIELD_INVALID",
        "message": "Age must be 18 or older.",
        "source": "User.Age"
      }
    ]
  }
}
```

**Customization**: The DTOs (`ResultResponse`, `PublicErrorItem`, `PublicErrorResponse`) are designed to be flexible and can be customized or extended to fit specific API design requirements.

# Error Results

`Error` provides structured failure metadata — `Code`, `Message`, `Source`, `Caller`, and `Exception` — to simplify control flow, support clear logging, and enable rich diagnostics.


## Error Properties

| Property    | Type         | Description                                    |
|-------------|--------------|------------------------------------------------|
| `Message`   | `string`     | Human-readable error description               |
| `Code`      | `string`     | Unique error identifier (e.g., `USER_LOCKED`). Best practice: use consistent naming (e.g., `UPPER_SNAKE_CASE`) and ensure uniqueness. |
| `Source`    | `string?`    | Logical component or module that caused error  |
| `Caller`    | `string?`    | Method name where the error was created. Often automatically populated using `[CallerMemberName]` attribute. |
| `Exception` | `Exception?` | Captured exception (optional)                  |


### Example

```csharp
// Direct error creation
var error = new Error("Unauthorized", "AUTH_ERROR", "AuthService", nameof(Login));
Error simple = "Something went wrong"; // Implicit conversion

// Example output:
// Error { Code = "AUTH_ERROR", Message = "Unauthorized", Source = "AuthService", Caller = "Login" }

public Result<User> Authenticate(string email, string password)
{
    return userRepository.FindByEmail(email)
        .FailIfNull("USER_NOT_FOUND", "User not found", source: "AuthService")
        .FailIf(u => !u.PasswordMatches(password), "INVALID_PASSWORD", "Incorrect password", "AuthService")
        .Ensure(u => u.IsActive, "USER_INACTIVE");
}

// Example failed output:
// Error { Code = "INVALID_PASSWORD", Message = "Incorrect password", Source = "AuthService", Caller = "Authenticate" }
```
---
### Domain-Specific Errors: Validation

You can create strongly-typed `Error` subclasses to encapsulate business rules or validation logic unique to a domain (e.g., user management, payments, inventory). These enrich error reporting while keeping your domain model expressive and type-safe:

```csharp
public sealed class ValidationError : Error
{
    public string FieldName { get; }
    public ValidationError(string field, string message)
        : base(message, "VALIDATION_ERROR", source: $"Field:{field}", caller: nameof(ValidateUser))
    {
        FieldName = field;
    }
}

public Result<User> ValidateUser(User user) =>
    Result.Success(user)
        .FailIf(u => string.IsNullOrWhiteSpace(u.Email), new ValidationError("Email", "Email is required"))
        .FailIf(u => u.Age < 18, new ValidationError("Age", "User must be 18 or older"))
        .OnFailure(error =>
        {
            if (error is ValidationError validation)
            {
                var traceId = Guid.NewGuid().ToString();
                LogError(error, traceId); // 🔗 Linked to structured logging example
            }
        });

// Example output:
// Error { Code = "VALIDATION_ERROR", Message = "Email is required", Source = "Field:Email", Caller = "ValidateUser" }

```
---
### Logging with Correlation & Context
```csharp
void LogError(Error error, string traceId)
{
    logger.LogError("[{TraceId}] ❗ {Code} | {Message} | {Source}.{Caller}",
        traceId, error.Code, error.Message, error.Source, error.Caller);

    if (error.Exception is not null)
        logger.LogError(error.Exception, "[{TraceId}] Exception Trace", traceId);
}
```

### Error Enrichment with Context
Use `.WithContext()` to add or override context dynamically, especially when errors propagate across layers.

```csharp
return Result.Fail<string>(
    new Error("Timeout", "TIMEOUT").WithContext("FetchExternalData", "IntegrationService")
);


result.OnFailure(err =>
{
    var enriched = err.WithContext(nameof(HandleError), "AlertingService");
    LogError(enriched, correlationId);
});

// Example output:
// Error { Code = "VALIDATION_ERROR", Message = "Email is required", Source = "Field:Email", Caller = "ValidateUser" }
```

### Best usage practices:
| Practice                                    			| Why                        |
| ------------------------------------------------------| -------------------------- |
| Use `.FailIf()` for early exits             			| Guard logic stays readable |
| Use `.Ensure()` for business invariants     			| Centralizes domain rules   |
| Prefer .Try() or .ThenTry() for fallible operations 	| Avoids manual try/catch    |
| Avoid mixing sync & async                   			| Prevents type confusion    |
| Always include `Code`, `Source`, `Caller`   			| For traceability           |
| Use `.WithContext()` when bubbling errors   			| Adds clarity across layers |
| Use `Result<Unit>` instead of bare `Result` 			| Enables fluent consistency |


## Fluent Extensions

FxResult provides a rich set of fluent extension methods for transforming, validating, branching, and reacting to `Result<T>` outcomes.

Each extension is grouped by its role in the pipeline, and every method supports both sync and async flows where applicable.

**How Result Chains Work**

FxResult pipelines operate with short-circuiting semantics:

- Each step (`Then`, `Tap`, `FailIf`, `Ensure`, etc.) is only executed if the current `Result` is successful (`IsSuccess == true`)

- Once an error occurs (via `.FailIf(...)`, `.ThenTry(...)`, etc.), the chain stops executing further transformations

- The error result is preserved and passed through to the end of the chain unchanged

- Side-effect hooks like `.OnFailure()` and `.OnFinally()` still run, enabling logging, auditing, or cleanup

This ensures:
    - predictable execution
    - no side effects after failure
    - clean, readable logic without manual branching

**Example**
```csharp
var result = Result.Success(5)
    .Then(x => x * 2)                         // → 10
    .FailIf(x => x == 10, "BLOCK", "Stop")    // → Failure here
    .Then(x => x + 1)                         // ❌ Skipped
    .Tap(x => Console.WriteLine(x))           // ❌ Skipped
    .OnFailure(err => Log(err))               // ✅ Executed
    .OnFinally(_ => Console.WriteLine("Done"));// ✅ Always runs
```


### Then: Value Transformation
Use to map or transform the successful result.

```csharp
var res = Result.Success(10)
    .Then(x => x * 2); // returns 20
```

**Variants**

- `.Then(...)` — simple transform
- `.Then(x => Result<T>)` — chained operation
- `.ThenTry(...)` — exception-safe version
- `.ThenAsync(...)` / `.ThenTryAsync(...)` — for `Task<Result<T>>` support
- `out var` — capture intermediate result

### Tap: Side Effects & Capture
Use to log, trace, publish events, or inspect without changing the value. `Tap` methods are designed for side-effects and always return the original `Result` (or a new `Result` if an exception occurs within the `Action`).
Can also be used to capture value or result via `out`, especially after async calls.

```csharp
var result = Result.Success(10)
    .Then(x => x * 2)                         // → 20
    .Tap(x => logger.Log($"Value is {x}"))    // Logs: Value is 20
    .Tap(out var capturedValue);              // capturedValue = 20

// Final result.Value = 20
```

### FailIf: Guard Conditions
Use to reject values based on rule violations or preconditions. `FailIf` is ideal for pre-conditions (fail if something is true).

```csharp
.FailIf(x => x < 0, "NEGATIVE", "Must be >= 0")
```

**Variants**
- `.FailIf(predicate, code, message)`
- `.FailIf(condition, code, message)`
- `.FailIfNull(...)` — convert nullables to failure
- `.FailIfAsync(...)` — for async predicates

### Ensure: Business Invariants
Use to enforce core domain rules (e.g., "user must be active"). `Ensure` is ideal for post-conditions or invariants (fail if something is *not* true).

```csharp
.Ensure(u => u.IsActive, "INACTIVE", "User is not active")
```

**Variants**
- `.Ensure(predicate, code, message)`
- `.EnsureAsync(...)` — async checks

### Try: Exception-Safe Factory
Use `Result.Try(...)` or `Result.TryAsync(...)` to wrap any fallible logic (e.g., parsing, deserialization, I/O, external calls) into a `Result<T>` without using `try/catch` in your business flow.

```csharp
// Synchronous
var result = Result<string>.Try(() => File.ReadAllText("file.txt"));

// Async
var result = await Result<string>.TryAsync(() => File.ReadAllTextAsync("file.txt"));

// Void return using Unit
var result = Result<Unit>.Try(() => File.Delete("temp.log"));
```

**Behavior**
- If the function runs successfully, a success Result<T> is returned
- If it throws, the exception is captured into an Error
- Supports Unit for Action and Func<Task> overloads
- Also usable as a starting point for fluent chains


### ThenTry: Exception-Safe Wrapping
Use to wrap potentially throwing code safely into a `Result`. `ThenTry` catches exceptions thrown by the wrapped operation and converts them into `Error` objects, maintaining the `Result` pipeline.

```csharp
.ThenTry(File.ReadAllText)
```

**Variants**
- `.ThenTry(...)`
- `.ThenTryAsync(...)` for any unsafe sync/async operations


### OnSuccess, OnFailure, OnFinally: Pipeline Outcome Hooks
Use these extensions to react to pipeline outcomes without breaking the flow.
They enable logging, recovery, auditing, and tracing after core logic execution.

| Method         | When it runs          | Can modify result? | Exception-safe?                                  |
| -------------- | --------------------- | ------------------ | ------------------------------------------------ |
| `.OnSuccess()` | Only on success       | Yes                | Yes *(guarded)*                                  |
| `.OnFailure()` | Only on failure       | Yes                | No *(exceptions propagate to global error handler)* |
| `.OnFinally()` | Always (success/fail) | Yes                | No *(exceptions propagate to global error handler)* |

**Variants**
- All available in sync/async flavours
- Safe and fluent for tracing and metrics

**Example of Chained Extensions**
```csharp
var result = await Result.Success("path/to/file")
    .ThenTryAsync(File.ReadAllTextAsync)
    .TapAsync(LogAsync)
    .EnsureAsync(IsJson, "INVALID_FORMAT", "Expected JSON")
    .Then(ParseJson)
    .OnSuccess(r => LogInfo(r.Value))
	.OnFailure(r => HandleError(r.Error))
    .OnFinally(() => Log("Completed read"));
```
---
## Ideal Use Cases

- **ASP.NET Core APIs**: Ensures consistent API responses for success and failure.
- **CQRS Command/Query Handlers**: Provides a clear and testable flow for business operations.
- **Functional Service-Layer Design**: Promotes pure functions and explicit error handling.
- **Message Consumers (MassTransit, Kafka, etc.)**: Enables robust processing of messages with clear error paths.
- **Background Jobs with Retry + Audit**: Facilitates reliable execution and tracking of long-running tasks.


## 👤 Author

Developed with scalability, maintainability, and clarity in mind. Contributions and feedback are welcome.
By Muhsin Meydan


## 🛠 License

MIT License. Use freely in commercial or OSS projects.


