# Results

[![Nuget downloads](https://img.shields.io/nuget/v/slr.results.svg)](https://www.nuget.org/packages/SLR.Results/) [![Nuget](https://img.shields.io/nuget/dt/slr.results)](https://www.nuget.org/packages/SLR.Results/) [![Build](https://github.com/stianleroux/Results/actions/workflows/dotnet.yml/badge.svg)](https://github.com/stianleroux/Results/actions/workflows/dotnet.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/stianleroux/Results/blob/master/LICENSE)

**SLR.Results is a .NET library that is designed to tackle a common issue. It presents an object that reflects the success or failure of an operation, as opposed to utilizing or raising exceptions.**

You can install [SLR.Results with NuGet](https://www.nuget.org/packages/SLR.Results/):

```
Install-Package SLR.Results
```

## GitHub Copilot Instructions

To help GitHub Copilot better understand and use the SLR.Results package in your C# projects, add these instructions to your `.github/copilot-instructions.md` file:

```markdown
# SLR.Results Usage Instructions

When working with methods that can fail or return errors, use the SLR.Results pattern:

## Return Types
- Use `Result` for operations without a return value
- Use `Result<T>` for operations that return a single value
- Use `Result<List<T>>` for operations that return collections with paging support

## Success Cases
```csharp
return Result.Success();
return Result<User>.Success(user);
return Result<List<User>>.Success(users, totalCount);
```

## Failure Cases
```csharp
return Result.Failure("Error message");
return Result<User>.Failure("User not found");
return Result<User>.NotFound("User not found");
return Result<User>.ValidationFailure("Invalid email format");
```

## Functional Composition
Chain operations using Bind, Map, and Match:
```csharp
return GetUser(userId)
    .Bind(user => UpdateUser(user))
    .Map(user => new UserDto(user))
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: err => BadRequest(err)
    );
```

## API Controllers
Use ApiResponseHelper for consistent API responses:
```csharp
return ApiResponseHelper.ResponseOutcome(
    await Mediator.Send(new GetUserQuery(id), cancellationToken), 
    this
);
```

Always prefer Result pattern over throwing exceptions for business logic errors.
```

### Cursor AI Instructions

For Cursor AI users, add these instructions to your `.cursorrules` file in the project root:

```
# SLR.Results Package Usage

When writing C# code that handles operations that can fail:

- Use Result for void operations: Result.Success() or Result.Failure("error")
- Use Result<T> for single value returns: Result<User>.Success(user) or Result<User>.Failure("error")
- Use Result<List<T>> for collections with paging: Result<List<User>>.Success(users, totalCount)

Error handling methods:
- .Failure("message") - general errors
- .NotFound("message") - not found errors (404)
- .ValidationError("message") - validation errors (400)

Functional composition:
- .Bind() - chain Result-producing operations
- .Map() - transform success values
- .MapError() - transform error values
- .Match() - pattern match on success/failure

In API Controllers, use:
ApiResponseHelper.ResponseOutcome(result, this)

Never throw exceptions for expected business logic errors. Always return Result types.
```

## Key Features

-   Works in most .NET Projects
-   Choose from Result, Result<T>, ListResult<T> to cater for all use cases
-   Store **multiple errors or validation errors** in one Result object
-   Store **Error or Success objects** instead of only error messages in string format
-   Allows uniformity in your code or anyone that needs to consume it
-   ApiResponseHelper to have uniform API Responses in Controllers
-   Result<List<T>> has built-in capability for paging using PagingExtensions
-   Monadic composition (Bind): Chain result-producing operations.
-   Mapping (Map, MapError): Transform values or errors.
-   Pattern matching (Match): Handle success/failure paths cleanly.
-   **New in v2:** Convenient helper methods - `IsFailure`, `GetValueOrThrow()`, `GetValueOrDefault()`, `WithData()`, `AddError()`, `CombineWith()`, implicit exception conversion
-   **New in v2:** Additional error types - `Unauthorized` and `Forbidden` for comprehensive HTTP status mapping

## Return a Result

A Result can store multiple Errors, NotFound, Unauthorized, Forbidden, or ValidationErrors.

```csharp
// return a result which indicates success
return Result.Success();

// return a result of a type which indicates success
return Result<Sample>.Success(new Sample());

//return a list result which indicates success
var entities = this.DatabaseContext.Samples.Select(x => x).AsNoTracking()
var count = await entities.Count();
var paged = await entities.ApplyPaging(model.PagingArgs).ToListAsync(cancellationToken);

return Result<List<Sample>>.Success(paged, count);

// return a result which indicates failure
return Result.Failure("Error");
return Result<List<Sample>>.Failure("Error");

// return validation failures
return Result<ProductDto>.ValidationFailure("Product name is required.");

var validationErrors = new Dictionary<string, List<string>>
{
    { "Name", new List<string> { "Product name is required." } },
    { "Price", new List<string> { "Price must be greater than 0." } }
};
return Result<ProductDto>.ValidationFailure(validationErrors);

// return specific error types
return Result<Product>.NotFound("Product not found");
return Result<User>.Unauthorized("User is not authenticated");
return Result<Document>.Forbidden("User does not have access to this document");

// method chaining for building results
var result = Result<User>.Success()
    .WithData(user)
    .AddError("Additional warning");

// get value with fallback
var user = Result<User>.Success(userData).GetValueOrDefault(defaultUser);

// get value or throw
var user = Result<User>.Success(userData).GetValueOrThrow(); // throws if failed

// combine multiple results
var result1 = ValidateProduct(product);
var result2 = CheckInventory(product);
var combined = result1.CombineWith(result2);

// implicit conversion from exception
Result<Product> result = new InvalidOperationException("Error occurred");
```

## Result on the API

This will handle 400, 404, 500 and 200

```csharp
  return ApiResponseHelper.ResponseOutcome(await this.Mediator.Send(new ExampleQuery(), cancellationToken), this)
```

## Monadic composition Helper Function

FunctionalResult class (like Map, Bind, MapError, and Match) are a great addition and fit perfectly with the Result<T> pattern you're already using.

```cs
Result<UserDto> result = GetUser(userId)
    .Bind(user => GetUser(user.Id))
    .Map(user => new UserDto(user.Name))
    .Match(
        onSuccess: dto => Ok(dto),
        onFailure: err => BadRequest(err)
    )
```

## New Helper Methods (v2)

### Checking Result Status
```csharp
// Check if result failed
if (result.IsFailure)
{
    // Handle error
}

// Get value or return default
var user = result.GetValueOrDefault(defaultUser);

// Get value or throw
var user = result.GetValueOrThrow(); // throws InvalidOperationException if failed
```

### Building and Chaining Results
```csharp
// Set data after creation
var result = Result<User>.Success()
    .WithData(user);

// Add errors incrementally
var result = Result<Product>.Success()
    .AddError("Warning 1")
    .AddError("Warning 2");

// Combine multiple results
var result1 = ValidateProduct(product);
var result2 = CheckInventory(product);
var combined = result1.CombineWith(result2);
```

### Exception Conversion
```csharp
// Implicit conversion from exception to Result
Result<Product> result = new InvalidOperationException("Database error");

// Or explicit via Failure
Result<Product> result = Result<Product>.Failure(exception);
```

## HTTP Status Mapping

The Result library now supports comprehensive HTTP status code mapping:

```csharp
// 200 OK
return Result<User>.Success(user);

// 400 Bad Request (Validation)
return Result<User>.ValidationFailure("Invalid email format");

// 404 Not Found
return Result<User>.NotFound("User not found");

// 401 Unauthorized
return Result<User>.Unauthorized("User is not authenticated");

// 403 Forbidden
return Result<Document>.Forbidden("User does not have access to this document");

// 500 Internal Server Error
return Result<User>.Failure("An unexpected error occurred");
```

## Security Best Practices

When using SLR.Results in production applications:

### Sensitive Data in Error Messages
- **Never include sensitive information** (passwords, tokens, PII, connection strings) in error messages
- Use generic error messages for external consumers
- Log detailed error information server-side only

```csharp
// ❌ BAD - Exposes sensitive data
return Result.Failure($"Database connection failed: {connectionString}");

// ✅ GOOD - Generic message for client, detailed logging server-side
_logger.LogError("Database connection failed: {ConnectionString}", connectionString);
return Result.Failure("Database connection error. Please contact support.");
```

### Important Note on Error Sanitization
**There is no built-in mechanism to automatically sanitize errors from Result objects.** Once error messages are added to a Result, they will be serialized and sent to clients via ApiResponseHelper. Therefore:

- **Be careful when creating Result objects** - avoid including sensitive data in error messages
- **Review exception messages** - exceptions may contain sensitive information like file paths or configuration details
- **Consider a middleware approach** - for additional safety, implement middleware to sanitize Result objects before they reach the client:

```csharp
// Example: Sanitizing middleware or filter
public class SanitizeResultFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && 
            objectResult.Value is Result result && 
            !result.IsSuccess)
        {
            // In production, replace detailed errors with generic messages
            if (_environment.IsProduction() && result.ErrorResult == ErrorResults.GeneralError)
            {
                result.Errors = ["An error occurred. Please contact support."];
                result.Message = "Error reference: " + Guid.NewGuid();
            }
        }
    }
}
```

### PII Redaction
The GitHub repository [stianleroux/Results](https://github.com/stianleroux/Results) does not have built-in functionality for PII redaction. To implement PII redaction using built-in .NET features, developers must integrate the **Microsoft.Extensions.Compliance.Redaction** package and configure redaction policies within their application's code. This involves defining data classifications, registering redaction services, and annotating sensitive data parameters in logging calls. For detailed instructions, visit [Microsoft Learn - Redact sensitive data](https://learn.microsoft.com/en-us/dotnet/core/extensions/redaction).

### Validation Errors
- Sanitize user input before including in validation error messages
- Avoid reflecting raw user input that could enable XSS attacks

```csharp
// ❌ BAD - Could reflect malicious input
return Result.ValidationError($"Invalid value: {userInput}");

// ✅ GOOD - Sanitized or generic message
return Result.ValidationError("Invalid email format");
```

### API Response Handling
- Use appropriate HTTP status codes via `ApiResponseHelper`
- Don't expose stack traces or internal errors to clients
- Implement proper error logging before returning results

```csharp
// ✅ GOOD - Structured error handling
var result = await _service.ProcessData(data);
if (!result.IsSuccess)
{
    _logger.LogWarning("Processing failed: {Errors}", result.Errors);
}
return ApiResponseHelper.ResponseOutcome(result, this);
```

### Thread Safety
- Result objects are immutable and thread-safe
- Safe to share across async operations and threads

## .NET Targeting

Latest .NET

## Contributors

Thanks to all the contributors and to all the people who gave feedback!

<a href="https://github.com/stianleroux/results/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=stianleroux/results" />
</a>

## Copyright

Copyright (c) Stian Le Roux. See LICENSE for details.
