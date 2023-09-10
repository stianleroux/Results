namespace Results.Models;

using System.Diagnostics.CodeAnalysis;
using Results.Constants;

[ExcludeFromCodeCoverage]
public class CollectionResult<T> : Result
{
    public IEnumerable<T> Data { get; set; }

    public int Count { get; set; }

    public static CollectionResult<T> Success(IEnumerable<T> data, int count, string? message = null) => new()
    {
        Data = data,
        Count = count,
        IsError = false,
        Message = message
    };

    public static new CollectionResult<T> Failure(string error, string? message = null) => new()
    {
        IsError = true,
        Errors = new List<string> { error },
        Message = message
    };

    public static new CollectionResult<T> Failure(List<string> errors, string? message = null) => new()
    {
        IsError = true,
        Errors = errors,
        Message = message
    };

    public new CollectionResult<T> AddError(string error)
    {
        this.IsError = true;
        this.Errors ??= new List<string>();
        this.Errors.Add(error);
        return this;
    }

    public static new CollectionResult<T> ValidationFailure(string validationError) => new()
    {
        IsValidationError = true,
        Message = validationError
    };

    public static new CollectionResult<T> ValidationFailure(Dictionary<string, List<string>> validationErrors) => new()
    {
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static CollectionResult<T> ValidationFailure(Dictionary<string, List<string>> validationErrors, IEnumerable<T>? data = default, int count = 0) => new()
    {
        Data = data,
        Count = count,
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static CollectionResult<T> ValidationFailure(Dictionary<string, List<string>> validationErrors, string message, IEnumerable<T>? data = default, int count = 0) => new()
    {
        Data = data,
        Message = message,
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static new CollectionResult<T> NotFound(string? message = null) => new()
    {
        IsError = false,
        IsValidationError = false,
        IsNotFound = true,
        Message = message
    };

    public void AddData(IEnumerable<T> data)
        => this.Data = data;

    public static (bool HasError, CollectionResult<T2> ErrorResult) HasError<T1, T2>(CollectionResult<T1> result) where T2 : class =>
    result == null || result.IsError || result.Data == null
        ? (true, CollectionResult<T2>.Failure(result?.Errors, result?.Message ?? ErrorConstants.InternalError))
        : result.IsNotFound
            ? (true, CollectionResult<T2>.NotFound(result.Message))
            : result.IsValidationError
                ? (true, CollectionResult<T2>.ValidationFailure(result.ValidationErrors, result.Message))
                : (false, new CollectionResult<T2>());
}
