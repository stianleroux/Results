# SLR.Results - Quick Reference Card

## Installation
```powershell
Install-Package SLR.Results
```

## Basic Usage

### Return Types
```csharp
Result                  // No data returned
Result<User>           // Single value
Result<List<User>>     // Collection with paging
```

### Creating Results
```csharp
// Success
Result.Success()
Result<User>.Success(user)
Result<List<User>>.Success(users, totalCount)

// Failure
Result.Failure("Error message")
Result<User>.NotFound("User not found")
Result<User>.Unauthorized("Not authenticated")
Result<User>.Forbidden("No access")
Result<User>.ValidationFailure("Invalid input")
Result<User>.ValidationFailure(validationErrorsDictionary)
```

### Checking Results
```csharp
if (result.IsSuccess) { /* use result.Data */ }
if (result.IsFailure) { /* handle result.Errors */ }
```

## Functional Composition

### Map - Transform Success Value
```csharp
Result<UserDto> GetUserDto(int id)
{
    return GetUser(id).Map(user => new UserDto(user));
}
```

### Bind - Chain Operations
```csharp
Result<Order> ProcessOrder(int userId)
{
    return GetUser(userId)
        .Bind(user => ValidateUser(user))
        .Bind(user => CreateOrder(user));
}
```

### Match - Handle Both Cases
```csharp
return GetUser(id).Match(
    onSuccess: user => Result<UserDto>.Success(new UserDto(user)),
    onFailure: errors => Result<UserDto>.Failure(errors)
);
```

### Async Variants
```csharp
await GetUserAsync(id)
    .MapAsync(user => new UserDto(user))
    .BindAsync(dto => ValidateDtoAsync(dto))
    .MatchAsync(
        onSuccess: async dto => await SaveAsync(dto),
        onFailure: async errors => await LogErrorsAsync(errors)
    );
```

## Database Operations

### With Data Return
```csharp
public async Task<Result<Product>> UpdateProduct(Product product)
{
    var rowsAffected = await _context.SaveChangesAsync();
    return DbResultProcessor<Product>.Outcome(
        product, 
        rowsAffected, 
        errorMessage: "Failed to update"
    );
}
```

### Without Data Return
```csharp
public async Task<Result> DeleteProduct(int id)
{
    var rowsAffected = await _context.SaveChangesAsync();
    return DbResultProcessor.Outcome(
        rowsAffected, 
        errorMessage: "Failed to delete"
    );
}
```

## API Controllers

### Standard Pattern
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Result<UserDto>>> GetUser(int id, CancellationToken ct)
{
    var result = await _mediator.Send(new GetUserQuery(id), ct);
    return ApiResponseHelper.ResponseOutcome(result, this);
}
```

### Extension Method
```csharp
[HttpPost]
public async Task<ActionResult<Result<User>>> CreateUser(CreateUserCommand command)
{
    var result = await _service.CreateUser(command);
    return result.ToActionResult();
}
```

## HTTP Status Codes
| Result Type | Method | HTTP Code |
|-------------|--------|-----------|
| Success | `.Success()` | 200 OK |
| Validation Error | `.ValidationFailure()` | 400 Bad Request |
| Not Found | `.NotFound()` | 404 Not Found |
| Unauthorized | `.Unauthorized()` | 401 Unauthorized |
| Forbidden | `.Forbidden()` | 403 Forbidden |
| General Error | `.Failure()` | 500 Internal Server Error |

## Paging

### Search Model
```csharp
public class ProductSearchModel : BaseSearchModel
{
    public string? Category { get; set; }
}
```

### Query with Paging
```csharp
var query = _context.Products.AsNoTracking();
var count = await query.CountAsync();
var products = await query
    .ApplyPaging(model.PagingArgs)
    .ToListAsync();
return Result<List<Product>>.Success(products, count);
```

## Helper Methods (v2+)

```csharp
result.IsFailure                        // Check if failed
result.GetValueOrDefault(defaultValue)  // Safe extraction
result.GetValueOrThrow()                // Throws if failed
result.WithData(data)                   // Set data (fluent)
result.AddError("error")                // Add error (fluent)
result1.CombineWith(result2)            // Combine results
Result<T> r = new Exception("msg")      // Implicit conversion
```

## Security Rules

### ? DO
- Return generic error messages to clients
- Log detailed errors server-side
- Sanitize validation errors
- Use appropriate error types

### ? DON'T
- Include connection strings in errors
- Include PII (emails, names) in errors
- Reflect raw user input in errors
- Expose stack traces to clients

## Common Patterns

### Repository
```csharp
Task<Result<T>> GetByIdAsync(int id, CancellationToken ct);
Task<Result<List<T>>> GetAllAsync(CancellationToken ct);
Task<Result<T>> CreateAsync(T entity, CancellationToken ct);
Task<Result> DeleteAsync(int id, CancellationToken ct);
```

### Service
```csharp
public async Task<Result<UserDto>> CreateUser(CreateUserCommand command)
{
    // 1. Validate
    var errors = Validate(command);
    if (errors.Any()) return Result<UserDto>.ValidationFailure(errors);
    
    // 2. Business rules
    var exists = await _repo.GetByEmailAsync(command.Email);
    if (exists.IsSuccess) return Result<UserDto>.Failure("Already exists");
    
    // 3. Create and map
    var user = new User(command);
    return await _repo.CreateAsync(user)
        .MapAsync(u => new UserDto(u));
}
```

### Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<UserDto>>> Get(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserQuery(id), ct);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
    
    [HttpPost]
    public async Task<ActionResult<Result<UserDto>>> Create(
        [FromBody] CreateUserCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
}
```

---

## When to Use What

| Scenario | Use This |
|----------|----------|
| Entity not found | `Result<T>.NotFound("message")` |
| Invalid input | `Result<T>.ValidationFailure(errors)` |
| User not logged in | `Result<T>.Unauthorized("message")` |
| User lacks permission | `Result<T>.Forbidden("message")` |
| System/unexpected error | `Result<T>.Failure("message")` |
| DB save operation | `DbResultProcessor<T>.Outcome(entity, rows)` |
| Chaining operations | `.Bind()` or `.BindAsync()` |
| Transforming data | `.Map()` or `.MapAsync()` |
| Pattern matching | `.Match(onSuccess, onFailure)` |
| API endpoint | `ApiResponseHelper.ResponseOutcome()` |
| Paginated query | `BaseSearchModel` + `.ApplyPaging()` |

---

**Remember**: The goal is to make error handling explicit, type-safe, and composable. Avoid exceptions for expected failures.
