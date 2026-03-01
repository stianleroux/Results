# SLR.Results - AI Code Assistant Instructions

This file provides instructions for AI code assistants (Claude Code, Windsurf, Cody, etc.) when working with the SLR.Results library.

---

## Fundamental Rule
**Never use exceptions for business logic failures.** The SLR.Results library provides a type-safe, composable alternative.

---

## Quick Reference

### Return Types
| Operation Type | Return Type | Example |
|----------------|-------------|---------|
| Void operation that can fail | `Result` | `Result DeleteUser(int id)` |
| Operation returning data | `Result<T>` | `Result<User> GetUser(int id)` |
| Collection with paging | `Result<List<T>>` | `Result<List<User>> GetUsers()` |

### Factory Methods
| Scenario | Method | HTTP Code |
|----------|--------|-----------|
| Success | `.Success()` | 200 |
| General error | `.Failure("msg")` | 500 |
| Not found | `.NotFound("msg")` | 404 |
| Validation error | `.ValidationFailure(errors)` | 400 |
| Not authenticated | `.Unauthorized("msg")` | 401 |
| No permission | `.Forbidden("msg")` | 403 |

### Checking Results
```csharp
if (result.IsSuccess) { /* use result.Data */ }
if (result.IsFailure) { /* handle result.Errors */ }
```

---

## Pattern Recognition

When you see code that:
1. **Throws exceptions** for business logic ? Refactor to return `Result`
2. **Returns null** for "not found" ? Use `Result<T>.NotFound()`
3. **Uses try-catch** for expected errors ? Use `Result<T>.Failure()`
4. **Returns bool success** ? Replace with `Result` or `Result<T>`
5. **Has out parameters** for errors ? Return `Result<T>` instead

---

## Code Generation Templates

### Repository Method
```csharp
public async Task<Result<{Entity}>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    var entity = await _context.{Entities}.FindAsync(id, cancellationToken);
    return entity == null 
        ? Result<{Entity}>.NotFound($"{Entity} {id} not found")
        : Result<{Entity}>.Success(entity);
}

public async Task<Result<{Entity}>> CreateAsync({Entity} entity, CancellationToken cancellationToken = default)
{
    await _context.{Entities}.AddAsync(entity, cancellationToken);
    var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
    return DbResultProcessor<{Entity}>.Outcome(entity, rowsAffected, "Failed to create {entity}");
}

public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
{
    var entity = await _context.{Entities}.FindAsync(id, cancellationToken);
    if (entity == null) return Result.NotFound($"{Entity} {id} not found");
    
    _context.{Entities}.Remove(entity);
    var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
    return DbResultProcessor.Outcome(rowsAffected, "Failed to delete {entity}");
}
```

### Service Method
```csharp
public async Task<Result<{Dto}>> Create{Entity}Async(Create{Entity}Command command, CancellationToken cancellationToken = default)
{
    // Validate input
    var validationErrors = Validate(command);
    if (validationErrors.Any())
        return Result<{Dto}>.ValidationFailure(validationErrors);
    
    // Check business rules
    var existingEntity = await _repository.GetByNameAsync(command.Name, cancellationToken);
    if (existingEntity.IsSuccess)
        return Result<{Dto}>.Failure("{Entity} with this name already exists");
    
    // Create entity
    var entity = new {Entity} { Name = command.Name, /* ... */ };
    var createResult = await _repository.CreateAsync(entity, cancellationToken);
    
    // Map to DTO
    return createResult.Map(e => new {Dto}(e));
}
```

### API Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class {Entity}Controller : ControllerBase
{
    private readonly IMediator _mediator;
    
    public {Entity}Controller(IMediator mediator) => _mediator = mediator;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<{Dto}>>> Get(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Get{Entity}Query(id), cancellationToken);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
    
    [HttpPost]
    public async Task<ActionResult<Result<{Dto}>>> Create([FromBody] Create{Entity}Command command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<Result<{Dto}>>> Update(int id, [FromBody] Update{Entity}Command command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Result<{Dto}>.ValidationFailure("ID mismatch").ToActionResult();
            
        var result = await _mediator.Send(command, cancellationToken);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Delete{Entity}Command(id), cancellationToken);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
    
    [HttpGet]
    public async Task<ActionResult<Result<List<{Dto}>>>> Search([FromQuery] {Entity}SearchModel model, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Search{Entity}Query(model), cancellationToken);
        return ApiResponseHelper.ResponseOutcome(result, this);
    }
}
```

### MediatR Handler (Query)
```csharp
public class Get{Entity}QueryHandler : IRequestHandler<Get{Entity}Query, Result<{Dto}>>
{
    private readonly I{Entity}Repository _repository;
    
    public Get{Entity}QueryHandler(I{Entity}Repository repository) => _repository = repository;
    
    public async Task<Result<{Dto}>> Handle(Get{Entity}Query request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken)
            .MapAsync(entity => new {Dto}(entity));
    }
}
```

### MediatR Handler (Command)
```csharp
public class Create{Entity}CommandHandler : IRequestHandler<Create{Entity}Command, Result<{Dto}>>
{
    private readonly I{Entity}Repository _repository;
    private readonly IValidator<Create{Entity}Command> _validator;
    
    public Create{Entity}CommandHandler(
        I{Entity}Repository repository, 
        IValidator<Create{Entity}Command> validator)
    {
        _repository = repository;
        _validator = validator;
    }
    
    public async Task<Result<{Dto}>> Handle(Create{Entity}Command request, CancellationToken cancellationToken)
    {
        // Validate
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e.ErrorMessage).ToList()
                );
            return Result<{Dto}>.ValidationFailure(errors);
        }
        
        // Create entity
        var entity = new {Entity} { /* map from command */ };
        var createResult = await _repository.CreateAsync(entity, cancellationToken);
        
        // Map to DTO
        return createResult.Map(e => new {Dto}(e));
    }
}
```

---

## Composition Patterns

### Sequential Operations (Bind Chain)
When each step depends on the previous:
```csharp
return await GetUserAsync(userId)
    .BindAsync(user => ValidateUserAsync(user))
    .BindAsync(user => CreateOrderAsync(user, command))
    .BindAsync(order => ProcessPaymentAsync(order))
    .MapAsync(order => MapToDto(order));
```

### Parallel Operations with Combination
When operations are independent:
```csharp
var userResult = await GetUserAsync(userId);
var productResult = await GetProductAsync(productId);

if (!userResult.IsSuccess) return Result<Order>.Failure(userResult.Errors);
if (!productResult.IsSuccess) return Result<Order>.Failure(productResult.Errors);

// Or combine them
var combined = userResult.CombineWith(productResult);
if (!combined.IsSuccess) return Result<Order>.Failure(combined.Errors);

return await CreateOrderAsync(userResult.Data, productResult.Data);
```

### Conditional Logic
```csharp
public async Task<Result<Order>> ProcessOrder(int userId, OrderCommand command)
{
    var userResult = await GetUserAsync(userId);
    if (!userResult.IsSuccess)
        return Result<Order>.Failure(userResult.Errors);
    
    var user = userResult.Data;
    
    if (!user.IsVerified)
        return Result<Order>.Forbidden("User must be verified to place orders");
    
    if (user.Balance < command.TotalAmount)
        return Result<Order>.ValidationFailure("Insufficient balance");
    
    return await CreateOrderAsync(user, command);
}
```

---

## Paging Implementation

### Step 1: Create Search Model
```csharp
public class {Entity}SearchModel : BaseSearchModel
{
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
```

### Step 2: Apply Filters and Paging
```csharp
public async Task<Result<List<{Dto}>>> SearchAsync({Entity}SearchModel model, CancellationToken cancellationToken = default)
{
    var query = _context.{Entities}.AsNoTracking();
    
    // Apply filters
    if (!string.IsNullOrEmpty(model.SearchTerm))
        query = query.Where(e => e.Name.Contains(model.SearchTerm));
    
    if (model.FromDate.HasValue)
        query = query.Where(e => e.CreatedDate >= model.FromDate.Value);
    
    // Get count BEFORE paging
    var count = await query.CountAsync(cancellationToken);
    
    // Apply paging (includes sorting)
    var entities = await query
        .ApplyPaging(model.PagingArgs)
        .Select(e => new {Dto}(e))
        .ToListAsync(cancellationToken);
    
    return Result<List<{Dto}>>.Success(entities, count);
}
```

---

## Error Handling Guidelines

### In Repository Layer
- Return `NotFound` when entity doesn't exist
- Use `DbResultProcessor` for save operations
- Never throw exceptions

### In Service Layer
- Return `ValidationFailure` for invalid input
- Return `Failure` for business rule violations
- Use functional composition to chain operations
- Log errors before returning results

### In API Layer
- Use `ApiResponseHelper.ResponseOutcome()` for all endpoints
- Let the Result system handle HTTP status codes
- Don't manually create `ObjectResult`, `NotFoundResult`, etc.

### Logging Pattern
```csharp
var result = await _repository.GetByIdAsync(id);
if (!result.IsSuccess)
{
    _logger.LogWarning("Failed to get {Entity} {Id}: {Errors}", 
        nameof({Entity}), id, string.Join(", ", result.Errors));
}
return result;
```

---

## Security Checklist

When creating Result objects:
- [ ] No connection strings in error messages
- [ ] No file paths in error messages
- [ ] No PII (emails, names, SSNs) in error messages
- [ ] No stack traces in error messages (log separately)
- [ ] No raw user input reflected in errors
- [ ] Generic client messages with detailed server logs
- [ ] Validation errors don't expose system internals

Safe error pattern:
```csharp
try
{
    // operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Detailed error with context: {Context}", context);
    return Result.Failure("A system error occurred. Reference: " + Guid.NewGuid());
}
```

---

## Testing Patterns

### Success Test
```csharp
[Fact]
public async Task {Method}_When{Condition}_ReturnsSuccess()
{
    // Arrange
    var input = /* test data */;
    
    // Act
    var result = await _sut.{Method}(input);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Errors);
    Assert.Equal(expected, result.Data.Property);
}
```

### Failure Test
```csharp
[Fact]
public async Task {Method}_When{Condition}_Returns{ErrorType}()
{
    // Arrange
    var input = /* invalid data */;
    
    // Act
    var result = await _sut.{Method}(input);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailure);
    Assert.Equal(ErrorResults.{ErrorType}, result.ErrorResult);
    Assert.NotEmpty(result.Errors);
}
```

---

## Common Mistakes to Avoid

### Mistake 1: Accessing Data Without Checking Success
```csharp
// ? WRONG
var result = await GetUser(id);
var userName = result.Data.Name; // NullReferenceException if failed!

// ? CORRECT
var result = await GetUser(id);
if (result.IsSuccess)
{
    var userName = result.Data.Name;
}

// ? ALSO CORRECT
var userName = result.GetValueOrDefault(new User { Name = "Unknown" }).Name;
```

### Mistake 2: Mixing Exceptions and Results
```csharp
// ? WRONG
public Result<User> GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid ID");
    return Result<User>.Success(user);
}

// ? CORRECT
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return Result<User>.ValidationFailure("ID must be greater than 0");
    return Result<User>.Success(user);
}
```

### Mistake 3: Not Using Appropriate Error Types
```csharp
// ? WRONG - Generic failure for specific cases
if (user == null) return Result<User>.Failure("Not found");

// ? CORRECT - Specific error type
if (user == null) return Result<User>.NotFound("User not found");
```

### Mistake 4: Breaking the Composition Chain
```csharp
// ? WRONG - Loses Result context
var userResult = await GetUserAsync(id);
if (!userResult.IsSuccess) return /* error */;
var user = userResult.Data;

var validationResult = await ValidateUserAsync(user);
if (!validationResult.IsSuccess) return /* error */;

// ? CORRECT - Use Bind for composition
return await GetUserAsync(id)
    .BindAsync(user => ValidateUserAsync(user))
    .BindAsync(user => ProcessUserAsync(user));
```

### Mistake 5: Not Passing CancellationToken
```csharp
// ? WRONG
public async Task<Result<User>> GetUserAsync(int id)
{
    return await _repository.GetByIdAsync(id); // Missing CT
}

// ? CORRECT
public async Task<Result<User>> GetUserAsync(int id, CancellationToken cancellationToken = default)
{
    return await _repository.GetByIdAsync(id, cancellationToken);
}
```

---

## Integration Examples

### FluentValidation Integration
```csharp
public async Task<Result<{Entity}>> ValidateAndCreate(Create{Entity}Command command)
{
    var validationResult = await _validator.ValidateAsync(command);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToList()
            );
        return Result<{Entity}>.ValidationFailure(errors);
    }
    
    return await _repository.CreateAsync(new {Entity}(command));
}
```

### AutoMapper Integration
```csharp
public async Task<Result<{Dto}>> Get{Entity}(int id)
{
    return await _repository.GetByIdAsync(id)
        .MapAsync(entity => _mapper.Map<{Dto}>(entity));
}
```

### EF Core Integration
```csharp
public async Task<Result<List<ProductDto>>> GetProducts(ProductSearchModel model)
{
    var query = _context.Products
        .AsNoTracking()
        .Where(p => p.IsActive);
    
    if (!string.IsNullOrEmpty(model.Category))
        query = query.Where(p => p.Category == model.Category);
    
    var count = await query.CountAsync();
    var products = await query
        .ApplyPaging(model.PagingArgs)
        .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
    
    return Result<List<ProductDto>>.Success(products, count);
}
```

---

## Decision Flow for AI Agents

```
START: User requests code generation/modification
  ?
Can this operation fail?
  NO ? Return T directly
  YES ?
  
Does it return data?
  NO ? Use Result
  YES ? Use Result<T>
  ?
  
What can cause failure?
  - Invalid input ? ValidationFailure()
  - Not found ? NotFound()
  - No auth ? Unauthorized()
  - No permission ? Forbidden()
  - System error ? Failure()
  ?
  
Is this chaining operations?
  NO ? Return single Result
  YES ?
  
Next operation returns Result?
  YES ? Use .Bind() or .BindAsync()
  NO ? Use .Map() or .MapAsync()
  ?
  
Is this an API endpoint?
  YES ? Wrap with ApiResponseHelper.ResponseOutcome()
  NO ? Return Result directly
  ?
END: Generate code
```

---

## Compatibility Notes

- **Target Framework**: .NET Standard 2.0 (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)
- **C# Version**: Uses modern C# features (nullable reference types, collection expressions)
- **Dependencies**:
  - Microsoft.AspNetCore.Mvc.Core (for API helpers)
  - Newtonsoft.Json (for serialization)
  - System.Text.Json (for modern serialization)

---

## Package Information

- **NuGet**: `SLR.Results`
- **GitHub**: https://github.com/stianleroux/Results
- **License**: MIT
- **Current Version**: 2.0.13

---

## Summary Checklist for Code Generation

When generating code, ensure:

? **Returns**: All fallible methods return `Result` or `Result<T>`  
? **Errors**: Use specific error types (NotFound, ValidationFailure, etc.)  
? **Database**: Use `DbResultProcessor` for EF Core operations  
? **Composition**: Use Bind/Map/Match for chaining  
? **API**: Use `ApiResponseHelper.ResponseOutcome()` in controllers  
? **Paging**: Inherit from `BaseSearchModel`, use `ApplyPaging()`  
? **Async**: Pass `CancellationToken` through all async calls  
? **Security**: Never expose sensitive data in error messages  
? **Testing**: Check `IsSuccess`/`IsFailure` in assertions  

? **No Exceptions**: Don't throw for business logic errors  
? **No Nulls**: Don't return null for "not found" - use Result.NotFound()  
? **No Mixed**: Don't mix exception-based and Result-based error handling  
? **No Direct Access**: Don't access `result.Data` without checking `IsSuccess`  

---

This pattern ensures consistent, type-safe, and testable error handling across your entire application.
