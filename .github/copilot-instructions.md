# SLR.Results Package - AI Agent Instructions

## Overview
The SLR.Results library implements the Result pattern for C# applications, providing a type-safe way to handle success and failure cases without exceptions. This pattern is essential for building robust, predictable APIs.

---

## Core Principles

### 1. **Never Throw Exceptions for Business Logic Errors**
- Exceptions should only be used for truly exceptional circumstances (system failures, infrastructure issues)
- Use Result types to represent expected failures (validation errors, not found, unauthorized, etc.)
- This makes error handling explicit and forces consumers to handle failures

### 2. **Always Use Result Types for Fallible Operations**
When a method can fail, return a Result type:
```csharp
// ? BAD - Uses exceptions for control flow
public User GetUser(int id)
{
    var user = _context.Users.Find(id);
    if (user == null) throw new NotFoundException("User not found");
    return user;
}

// ? GOOD - Uses Result pattern
public Result<User> GetUser(int id)
{
    var user = _context.Users.Find(id);
    return user == null 
        ? Result<User>.NotFound("User not found") 
        : Result<User>.Success(user);
}
```

---

## Result Types

### Generic Result<T>
For operations that return data:
```csharp
Result<User> GetUser(int id);
Result<List<Product>> GetProducts();
Result<OrderDto> CreateOrder(CreateOrderCommand command);
```

### Non-Generic Result
For operations without return data:
```csharp
Result DeleteUser(int id);
Result UpdateSettings(SettingsDto settings);
Result SendEmail(EmailRequest request);
```

---

## Factory Methods Reference

### Success Results
```csharp
// Simple success
Result.Success();
Result<User>.Success(user);

// Success with count (for paging)
Result<List<Product>>.Success(products, totalCount);

// Success with message
Result<Order>.Success(order, message: "Order created successfully");
```

### General Failure
```csharp
// Single error
Result.Failure("Operation failed");
Result<User>.Failure("User creation failed");

// Multiple errors
Result<Product>.Failure(new List<string> { "Error 1", "Error 2" });

// From exception
Result<Order>.Failure(exception);

// Implicit conversion from exception
Result<Product> result = new InvalidOperationException("Database error");
```

### Not Found (404)
```csharp
Result<User>.NotFound("User not found");
Result<Product>.NotFound($"Product {id} not found");
```

### Unauthorized (401)
```csharp
Result<Document>.Unauthorized("User is not authenticated");
Result<Resource>.Unauthorized("Invalid credentials");
```

### Forbidden (403)
```csharp
Result<Document>.Forbidden("User does not have access to this resource");
Result<AdminPanel>.Forbidden("Insufficient permissions");
```

### Validation Errors (400)
```csharp
// Single validation error
Result<Product>.ValidationFailure("Product name is required");

// Multiple validation errors
var validationErrors = new Dictionary<string, List<string>>
{
    { "Email", new List<string> { "Email is required", "Email format is invalid" } },
    { "Password", new List<string> { "Password must be at least 8 characters" } }
};
Result<User>.ValidationFailure(validationErrors);

// Validation failure with data
Result<User>.ValidationFailure(validationErrors, message: "Validation failed", data: partialUser);
```

---

## Database Operations

### Using DbResultProcessor<T>
For operations that return typed data:
```csharp
public async Task<Result<Product>> UpdateProduct(Product product)
{
    var rowsAffected = await _context.SaveChangesAsync();
    return DbResultProcessor<Product>.Outcome(
        product, 
        rowsAffected, 
        errorMessage: "Failed to update product"
    );
}
```

### Using DbResultProcessor (Non-Generic)
For operations without data payload:
```csharp
public async Task<Result> DeleteProduct(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null) return Result.NotFound("Product not found");
    
    _context.Products.Remove(product);
    var rowsAffected = await _context.SaveChangesAsync();
    
    return DbResultProcessor.Outcome(
        rowsAffected, 
        errorMessage: "Failed to delete product"
    );
}
```

---

## Functional Composition

### Map - Transform Success Values
Use when you need to transform the data inside a successful result:
```csharp
Result<UserDto> GetUserDto(int id)
{
    return GetUser(id)
        .Map(user => new UserDto 
        { 
            Id = user.Id, 
            Name = user.Name 
        });
}
```

### Bind - Chain Result-Producing Operations
Use when the next operation also returns a Result:
```csharp
Result<Order> ProcessOrder(int userId, CreateOrderCommand command)
{
    return GetUser(userId)
        .Bind(user => ValidateUser(user))
        .Bind(user => CreateOrder(user, command))
        .Bind(order => ProcessPayment(order));
}
```

### MapError - Transform Error Messages
Use to customize error messages:
```csharp
return GetUser(id)
    .MapError(errors => errors.Select(e => $"User Service: {e}").ToList());
```

### Match - Pattern Match on Success/Failure
Use to handle both success and failure cases:
```csharp
return GetUser(userId).Match(
    onSuccess: user => Result<UserDto>.Success(MapToDto(user)),
    onFailure: errors => Result<UserDto>.Failure(errors)
);
```

### Async Composition
All methods have async variants:
```csharp
public async Task<Result<OrderDto>> ProcessOrderAsync(int userId, CreateOrderCommand command)
{
    return await GetUserAsync(userId)
        .BindAsync(user => ValidateUserAsync(user))
        .BindAsync(user => CreateOrderAsync(user, command))
        .MapAsync(order => MapToDto(order));
}
```

---

## API Controller Integration

### Using ApiResponseHelper
Automatically maps Result to appropriate HTTP status codes:
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Result<UserDto>>> GetUser(int id, CancellationToken cancellationToken)
{
    var result = await _mediator.Send(new GetUserQuery(id), cancellationToken);
    return ApiResponseHelper.ResponseOutcome(result, this);
}
```

### Manual ActionResult Conversion
Using extension methods:
```csharp
[HttpPost]
public async Task<ActionResult<Result<User>>> CreateUser(CreateUserCommand command)
{
    var result = await _service.CreateUser(command);
    return result.ToActionResult();
}
```

### HTTP Status Code Mapping
- `Result.Success()` ? 200 OK
- `Result.ValidationFailure()` ? 400 Bad Request
- `Result.NotFound()` ? 404 Not Found
- `Result.Unauthorized()` ? 401 Unauthorized
- `Result.Forbidden()` ? 403 Forbidden
- `Result.Failure()` ? 500 Internal Server Error

---

## Common Patterns

### Repository Pattern
```csharp
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<List<User>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class UserRepository : IUserRepository
{
    public async Task<Result<User>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(id, cancellationToken);
        return user == null 
            ? Result<User>.NotFound($"User {id} not found")
            : Result<User>.Success(user);
    }

    public async Task<Result<User>> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
        return DbResultProcessor<User>.Outcome(user, rowsAffected);
    }
}
```

### Service Layer Pattern
```csharp
public class UserService
{
    public async Task<Result<UserDto>> CreateUser(CreateUserCommand command)
    {
        // Validate
        var validationResult = ValidateCommand(command);
        if (!validationResult.IsSuccess)
            return Result<UserDto>.ValidationFailure(validationResult.ValidationErrors);

        // Check if exists
        var existingUser = await _repository.GetByEmailAsync(command.Email);
        if (existingUser.IsSuccess)
            return Result<UserDto>.Failure("User with this email already exists");

        // Create
        var user = new User { Email = command.Email, Name = command.Name };
        var createResult = await _repository.CreateAsync(user);
        
        return createResult.Map(u => new UserDto { Id = u.Id, Email = u.Email });
    }
}
```

### CQRS with MediatR
```csharp
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken)
            .MapAsync(user => new UserDto(user));
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validationErrors = await ValidateAsync(request);
        if (validationErrors.Any())
            return Result<UserDto>.ValidationFailure(validationErrors);

        var user = new User(request.Email, request.Name);
        return await _repository.CreateAsync(user, cancellationToken)
            .MapAsync(u => new UserDto(u));
    }
}
```

### Paging with Result
```csharp
public async Task<Result<List<ProductDto>>> GetProducts(ProductSearchModel searchModel)
{
    var query = _context.Products.AsNoTracking();
    
    // Apply filters
    if (!string.IsNullOrEmpty(searchModel.SearchTerm))
        query = query.Where(p => p.Name.Contains(searchModel.SearchTerm));
    
    // Get count before paging
    var count = await query.CountAsync();
    
    // Apply paging
    var products = await query
        .ApplyPaging(searchModel.PagingArgs)
        .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
        .ToListAsync();
    
    return Result<List<ProductDto>>.Success(products, count);
}
```

---

## Advanced Usage

### Combining Multiple Results
```csharp
public async Task<Result<OrderDto>> CreateOrder(CreateOrderCommand command)
{
    var userResult = await GetUser(command.UserId);
    var productResult = await GetProduct(command.ProductId);
    
    // Combine results - fails if either fails
    var combined = userResult.CombineWith(productResult);
    if (!combined.IsSuccess)
        return Result<OrderDto>.Failure(combined.Errors);
    
    var order = new Order(userResult.Data, productResult.Data);
    return await _repository.CreateAsync(order)
        .MapAsync(o => MapToDto(o));
}
```

### Building Results Incrementally
```csharp
public Result<Report> GenerateReport(ReportConfig config)
{
    var result = Result<Report>.Success(new Report());
    
    if (!ValidateConfig(config))
        result.AddError("Invalid configuration");
    
    if (!CheckPermissions())
        result.AddError("Insufficient permissions");
    
    if (result.IsFailure)
        return result;
    
    return result.WithData(BuildReport(config));
}
```

### Safe Value Extraction
```csharp
// Get value or use default
var user = userResult.GetValueOrDefault(User.Anonymous);

// Get value or throw (use sparingly, only when you're certain of success)
try
{
    var user = userResult.GetValueOrThrow();
    ProcessUser(user);
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Unexpected failure");
}
```

---

## Anti-Patterns to Avoid

### ? Don't Mix Exceptions and Results
```csharp
// BAD - Inconsistent error handling
public Result<User> GetUser(int id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid ID"); // Should use Result.ValidationFailure
    
    return Result<User>.Success(user);
}
```

### ? Don't Ignore Failures
```csharp
// BAD - Ignores the result
var result = await _service.GetUser(id);
ProcessUser(result.Data); // Could be null if failed!

// GOOD - Check success first
var result = await _service.GetUser(id);
if (result.IsSuccess)
{
    ProcessUser(result.Data);
}
else
{
    _logger.LogWarning("Failed to get user: {Errors}", result.Errors);
}
```

### ? Don't Return Success with Error State
```csharp
// BAD - Conflicting state
return new Result<User> 
{ 
    Data = null, 
    ErrorResult = ErrorResults.None // Says success but has no data
};

// GOOD - Use appropriate factory methods
return Result<User>.NotFound("User not found");
```

### ? Don't Expose Sensitive Data in Errors
```csharp
// BAD - Exposes connection string
return Result.Failure($"Connection failed: {connectionString}");

// GOOD - Generic message, log details server-side
_logger.LogError("Connection failed: {ConnectionString}", connectionString);
return Result.Failure("Database connection error");
```

---

## Testing Result-Based Code

### Testing Success Cases
```csharp
[Fact]
public async Task GetUser_WhenExists_ReturnsSuccess()
{
    // Arrange
    var userId = 1;
    
    // Act
    var result = await _service.GetUser(userId);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(userId, result.Data.Id);
    Assert.Empty(result.Errors);
}
```

### Testing Failure Cases
```csharp
[Fact]
public async Task GetUser_WhenNotFound_ReturnsNotFound()
{
    // Arrange
    var userId = 999;
    
    // Act
    var result = await _service.GetUser(userId);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.True(result.IsFailure);
    Assert.Equal(ErrorResults.NotFound, result.ErrorResult);
    Assert.Null(result.Data);
}

[Fact]
public async Task CreateUser_WhenValidationFails_ReturnsValidationError()
{
    // Arrange
    var command = new CreateUserCommand { Email = "invalid-email" };
    
    // Act
    var result = await _service.CreateUser(command);
    
    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorResults.ValidationError, result.ErrorResult);
    Assert.NotEmpty(result.ValidationErrors);
    Assert.Contains("Email", result.ValidationErrors.Keys);
}
```

### Testing Functional Composition
```csharp
[Fact]
public async Task ProcessOrder_ChainsOperations_ReturnsSuccess()
{
    // Arrange
    var command = new CreateOrderCommand { UserId = 1, ProductId = 1 };
    
    // Act
    var result = await GetUser(command.UserId)
        .BindAsync(user => ValidateUser(user))
        .BindAsync(user => CreateOrder(user, command))
        .MapAsync(order => MapToDto(order));
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.IsType<OrderDto>(result.Data);
}
```

---

## API Controller Patterns

### Standard Controller Action
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Result<UserDto>>> GetUser(int id, CancellationToken cancellationToken)
{
    var result = await _mediator.Send(new GetUserQuery(id), cancellationToken);
    return ApiResponseHelper.ResponseOutcome(result, this);
}

[HttpPost]
public async Task<ActionResult<Result<UserDto>>> CreateUser(CreateUserCommand command, CancellationToken cancellationToken)
{
    var result = await _mediator.Send(command, cancellationToken);
    return ApiResponseHelper.ResponseOutcome(result, this);
}

[HttpDelete("{id}")]
public async Task<ActionResult<Result>> DeleteUser(int id, CancellationToken cancellationToken)
{
    var result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
    return ApiResponseHelper.ResponseOutcome(result, this);
}
```

### Manual ActionResult Conversion
```csharp
[HttpGet("search")]
public async Task<ActionResult<Result<List<ProductDto>>>> SearchProducts([FromQuery] ProductSearchModel model)
{
    var result = await _service.SearchProducts(model);
    return result.ToActionResult();
}
```

---

## Code Generation Guidance

### When Generating Methods
1. **Identify if the operation can fail** ? Use Result type
2. **Determine the return type** ? Use Result<T> for data, Result for void
3. **Use appropriate factory methods** based on failure type:
   - NotFound for missing resources
   - ValidationFailure for input validation
   - Unauthorized/Forbidden for auth failures
   - Failure for general errors
4. **Consider functional composition** if chaining multiple operations
5. **Never throw exceptions** for expected failures

### When Generating API Controllers
1. **Return ActionResult<Result<T>>** or **ActionResult<Result>**
2. **Use ApiResponseHelper.ResponseOutcome()** for consistent responses
3. **Pass CancellationToken** to all async operations
4. **Let Result handle error codes** - don't manually create ObjectResult

### When Generating Services
1. **Return Result types** from all public methods that can fail
2. **Use DbResultProcessor** for database operations
3. **Chain operations** with Bind when appropriate
4. **Validate early** and return ValidationFailure for invalid input
5. **Log errors** but keep error messages client-safe

### When Generating Repositories
1. **Return Result<T>** for single entity operations
2. **Return Result<List<T>>** for collections (with count for paging)
3. **Use NotFound** when entity doesn't exist
4. **Use DbResultProcessor** for save operations
5. **Use ApplyPaging()** extension for paged queries

---

## Paging Pattern

### BaseSearchModel
All search models should inherit from BaseSearchModel:
```csharp
public class ProductSearchModel : BaseSearchModel
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

### Applying Paging
```csharp
public async Task<Result<List<ProductDto>>> SearchProducts(ProductSearchModel searchModel)
{
    var query = _context.Products.AsNoTracking();
    
    // Apply filters
    if (!string.IsNullOrEmpty(searchModel.Category))
        query = query.Where(p => p.Category == searchModel.Category);
    
    if (searchModel.MinPrice.HasValue)
        query = query.Where(p => p.Price >= searchModel.MinPrice.Value);
    
    // Get total count
    var count = await query.CountAsync();
    
    // Apply paging and sorting
    var products = await query
        .ApplyPaging(searchModel.PagingArgs)
        .Select(p => new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price })
        .ToListAsync();
    
    return Result<List<ProductDto>>.Success(products, count);
}
```

---

## Security Best Practices

### Error Message Sanitization
```csharp
// ? BAD - Exposes internal details
catch (SqlException ex)
{
    return Result.Failure(ex.Message); // May contain connection strings, table names
}

// ? GOOD - Generic message, detailed logging
catch (SqlException ex)
{
    _logger.LogError(ex, "Database error occurred");
    return Result.Failure("A database error occurred. Please contact support.");
}
```

### PII and Sensitive Data
```csharp
// ? BAD - Includes PII in error
return Result.Failure($"User {user.Email} failed validation");

// ? GOOD - Generic message without PII
_logger.LogWarning("User {UserId} failed validation", user.Id);
return Result.ValidationFailure("User validation failed");
```

### Validation Error Messages
```csharp
// ? BAD - Reflects user input (XSS risk)
return Result.ValidationFailure($"Invalid value: {userInput}");

// ? GOOD - Structured, sanitized message
return Result.ValidationFailure("Email format is invalid");
```

---

## Quick Decision Tree

**Is the operation fallible?**
- No ? Return the value directly (T)
- Yes ? Continue

**Does it return data?**
- Yes ? Use Result<T>
- No ? Use Result

**What type of failure?**
- Input validation ? Use ValidationFailure()
- Resource not found ? Use NotFound()
- Auth required ? Use Unauthorized()
- Permission denied ? Use Forbidden()
- General/unexpected ? Use Failure()

**Chaining operations?**
- Next operation also returns Result ? Use Bind()
- Need to transform success value ? Use Map()
- Need to handle both paths ? Use Match()

**In an API Controller?**
- Yes ? Use ApiResponseHelper.ResponseOutcome() or .ToActionResult()
- No ? Return Result directly

---

## Summary for AI Agents

When generating C# code that uses SLR.Results:

1. ? Use Result types for all operations that can fail
2. ? Use appropriate factory methods (NotFound, ValidationFailure, etc.)
3. ? Use DbResultProcessor for database operations
4. ? Use functional composition (Map, Bind, Match) for chaining
5. ? Use ApiResponseHelper in controllers for consistent responses
6. ? Inherit from BaseSearchModel for search/paging scenarios
7. ? Use ApplyPaging() extension for paginated queries
8. ? Always pass CancellationToken in async methods
9. ? Never throw exceptions for business logic errors
10. ? Never expose sensitive data in error messages
11. ? Never ignore Result failures - always check IsSuccess
12. ? Never mix exception-based and Result-based error handling in the same layer

This pattern creates predictable, type-safe, and testable code that clearly communicates success and failure cases.
