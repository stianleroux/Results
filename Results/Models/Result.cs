namespace Results.Models;

using System.Diagnostics.CodeAnalysis;
using Results.Constants;

[ExcludeFromCodeCoverage]
public class Result
{
    public bool IsError { get; set; } = false;

    public bool IsValidationError { get; set; } = false;

    public bool IsNotFound { get; set; } = false;

    public string? Message { get; set; }

    public List<string> Errors { get; set; } = [];

    public Dictionary<string, List<string>> ValidationErrors { get; set; } = [];

    public static Result Success() => new()
    {
        IsError = false,
        Errors = [],
        ValidationErrors = []
    };

    public static Result Success(string message) => new()
    {
        IsError = false,
        Message = message,
        Errors = [],
        ValidationErrors = []
    };

    public static Result Failure(List<string> errors) => new()
    {
        IsError = true,
        Errors = errors,
        ValidationErrors = []
    };

    public static Result Failure(List<string> errors, string message) => new()
    {
        IsError = true,
        Errors = errors,
        Message = message,
        ValidationErrors = []
    };

    public static Result Failure(string error) => new()
    {
        IsError = true,
        Errors =
        [
            error,
        ],
        ValidationErrors = []
    };

    public static Result Failure(string error, string message) => new()
    {
        IsError = true,
        Message = message,
        Errors =
        [
            error,
        ],
        ValidationErrors = []
    };

    public static (bool HasError, Result? ErrorResult) HasError(Result result) => result switch
    {
        null => (true, Failure(ErrorConstants.InternalError)),
        { IsError: true } => (true, Failure(result.Errors, result.Message)),
        { IsNotFound: true } => (true, NotFound(result.Message)),
        { IsValidationError: true } => (true, ValidationFailure(result.ValidationErrors, result.Message)),
        _ => (false, new())
    };

    public static Result HandleResult(Result result) => result switch
    {
        { IsError: true } => Failure(result.Errors, result.Message),
        { IsValidationError: true } => ValidationFailure(result.ValidationErrors, result.Message),
        _ => Success()
    };

    public static Result ValidationFailure(Dictionary<string, List<string>> validationErrors) => new()
    {
        IsValidationError = true,
        Errors = [],
        ValidationErrors = validationErrors
    };

    public static Result ValidationFailure(Dictionary<string, List<string>> validationErrors, string message) => new()
    {
        IsValidationError = true,
        Message = message,
        Errors = [],
        ValidationErrors = validationErrors
    };

    public static Result ValidationFailure(string validationError) => new()
    {
        IsValidationError = true,
        Message = validationError
    };

    public static Result NotFound() => new()
    {
        IsError = false,
        IsValidationError = false,
        IsNotFound = true,
    };

    public static Result NotFound(string message) => new()
    {
        IsError = false,
        IsValidationError = false,
        IsNotFound = true,
        Message = message
    };

    public void AddError(string error)
    {
        this.IsError = true;
        this.Errors.Add(error);
    }

    public void AddValidationError(string name, List<string> errors)
    {
        this.IsValidationError = true;

        if (!this.ValidationErrors.TryGetValue(name, out var value))
        {
            this.ValidationErrors.Add(name, errors);
        }
        else
        {
            value.AddRange(errors);
        }
    }

    public bool HasError()
        => this.IsError || this.IsValidationError || this.IsNotFound;
}

[ExcludeFromCodeCoverage]
public class Result<T> : Result
{
    public T? Data { get; set; }

    public int Count { get; set; }

    public static Result<T> Success(T data, string? message = null) => new()
    {
        Data = data,
        IsError = false,
        Message = message
    };

    public static Result<T> Success(T data, int count, string? message = null) => new()
    {
        Data = data,
        Count = count,
        IsError = false,
        Message = message
    };

    public static new Result<T> Failure(string error, string? message = null) => new()
    {
        IsError = true,
        Errors = [error],
        Message = message
    };

    public static new Result<T> Failure(List<string> errors, string? message = null) => new()
    {
        IsError = true,
        Errors = errors,
        Message = message
    };

    public new Result<T> AddError(string error)
    {
        this.IsError = true;
        this.Errors ??= [];
        this.Errors.Add(error);
        return this;
    }

    public static new Result<T> ValidationFailure(string validationError) => new()
    {
        IsValidationError = true,
        Message = validationError
    };

    public static new Result<T> ValidationFailure(Dictionary<string, List<string>> validationErrors) => new()
    {
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static Result<T> ValidationFailure(Dictionary<string, List<string>> validationErrors, T data = default) => new()
    {
        Data = data,
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static Result<T> ValidationFailure(Dictionary<string, List<string>> validationErrors, string message, T data = default) => new()
    {
        Data = data,
        Message = message,
        IsValidationError = true,
        ValidationErrors = validationErrors
    };

    public static new Result<T> NotFound(string? message = null) => new()
    {
        IsError = false,
        IsValidationError = false,
        IsNotFound = true,
        Message = message
    };

    public void AddData(T data)
        => this.Data = data;

    public static (bool HasError, Result<T2> ErrorResult) HasError<T1, T2>(Result<T1> result) where T2 : class =>
    result == null || result.IsError || result.Data == null
        ? (true, Result<T2>.Failure(result?.Errors, result?.Message ?? ErrorConstants.InternalError))
        : result.IsNotFound
            ? (true, Result<T2>.NotFound(result.Message))
            : result.IsValidationError
                ? (true, Result<T2>.ValidationFailure(result.ValidationErrors, result.Message))
                : (false, new Result<T2>());
}