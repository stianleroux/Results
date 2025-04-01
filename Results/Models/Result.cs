namespace Results.Models;

using System.Diagnostics.CodeAnalysis;
using Results.Enums;

[ExcludeFromCodeCoverage]
public class Result
{
    public ErrorResults ErrorResult { get; set; } = ErrorResults.None;

    public string? Message { get; set; }

    public List<string>? Errors { get; set; } = [];

    public Dictionary<string, List<string>>? ValidationErrors { get; set; } = [];

    public static Result Success() => new()
    {
        ErrorResult = ErrorResults.None,
        Errors = [],
        ValidationErrors = []
    };

    public static Result Success(string? message) => new()
    {
        ErrorResult = ErrorResults.None,
        Message = message,
        Errors = [],
        ValidationErrors = []
    };

    public static Result Failure(List<string>? errors) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = errors ?? [],
        ValidationErrors = []
    };

    public static Result Failure(List<string>? errors, string? message) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = errors ?? [],
        Message = message,
        ValidationErrors = []
    };

    public static Result Failure(string? error) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = [error],
        ValidationErrors = []
    };

    public static Result Failure(string error, string message) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Message = message,
        Errors = [error],
        ValidationErrors = []
    };

    public static Result ValidationFailure(Dictionary<string, List<string>> validationErrors)
        => new()
        {
            ErrorResult = ErrorResults.ValidationError,
            Errors = [],
            ValidationErrors = validationErrors
        };

    public static Result ValidationFailure(Dictionary<string, List<string>>? validationErrors, string? message) => new()
    {
        ErrorResult = ErrorResults.ValidationError,
        Message = message,
        Errors = [],
        ValidationErrors = validationErrors ?? []
    };

    public static Result ValidationFailure(string validationError) => new()
    {
        ErrorResult = ErrorResults.ValidationError,
        Message = validationError
    };

    public static Result NotFound() => new()
    {
        ErrorResult = ErrorResults.NotFound
    };

    public static Result NotFound(string? message) => new()
    {
        ErrorResult = ErrorResults.NotFound,
        Message = message
    };

    public void AddError(string error)
    {
        this.ErrorResult = ErrorResults.GeneralError;
        this.Errors ??= [];
        this.Errors.Add(error);
    }

    public void AddValidationError(string name, List<string> errors)
    {
        this.ErrorResult = ErrorResults.ValidationError;
        this.ValidationErrors ??= [];
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
        => this.ErrorResult != ErrorResults.None;
}

public class Result<T>
{
    public ErrorResults ErrorResult { get; set; } = ErrorResults.None;

    public string? Message { get; set; }

    public T? Data { get; set; }

    public int Count { get; set; }

    public List<string>? Errors { get; set; } = [];

    public Dictionary<string, List<string>>? ValidationErrors { get; set; } = [];

    public static Result<T> Success() => new()
    {
        ErrorResult = ErrorResults.None,
        Errors = [],
        ValidationErrors = []
    };

    public static Result<T> Success(T data) => new()
    {
        Data = data,
        Count = 0,
        ErrorResult = ErrorResults.None,
        Errors = [],
        ValidationErrors = []
    };

    public static Result<T> Success(T data, int count, string? message = "") => new()
    {
        Data = data,
        Count = count,
        ErrorResult = ErrorResults.None,
        Message = message,
        Errors = [],
        ValidationErrors = []
    };

    public static Result<T> Failure(List<string>? errors) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = errors ?? [],
        ValidationErrors = []
    };

    public static Result<T> Failure(List<string>? errors, string? message) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = errors ?? [],
        Message = message,
        ValidationErrors = []
    };

    public static Result<T> Failure(string? error) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Errors = [error],
        ValidationErrors = []
    };

    public static Result<T> Failure(string error, string message) => new()
    {
        ErrorResult = ErrorResults.GeneralError,
        Message = message,
        Errors = [error],
        ValidationErrors = []
    };

    public static Result<T> ValidationFailure(Dictionary<string, List<string>> validationErrors)
        => new()
        {
            ErrorResult = ErrorResults.ValidationError,
            Errors = [],
            ValidationErrors = validationErrors
        };

    public static Result<T> ValidationFailure(Dictionary<string, List<string>>? validationErrors, string? message) => new()
    {
        ErrorResult = ErrorResults.ValidationError,
        Message = message,
        Errors = [],
        ValidationErrors = validationErrors ?? []
    };

    public static Result<T> ValidationFailure(string validationError) => new()
    {
        ErrorResult = ErrorResults.ValidationError,
        Message = validationError
    };

    public static Result<T> NotFound() => new()
    {
        ErrorResult = ErrorResults.NotFound
    };

    public static Result<T> NotFound(string? message) => new()
    {
        ErrorResult = ErrorResults.NotFound,
        Message = message
    };

    public void AddError(string error)
    {
        this.ErrorResult = ErrorResults.GeneralError;
        this.Errors ??= [];
        this.Errors.Add(error);
    }

    public void AddValidationError(string name, List<string> errors)
    {
        this.ErrorResult = ErrorResults.ValidationError;
        this.ValidationErrors ??= [];
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
        => this.ErrorResult != ErrorResults.None;
}