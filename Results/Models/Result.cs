namespace Results.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Results.Enums;

/// <summary>
/// Represents a generic result object that encapsulates the outcome of an operation, including success/failure state, 
/// error information, and optional data.
/// </summary>
/// <remarks>
/// The <see cref="Result{T}"/> class provides a standardized way to return operation results with built-in support for:
/// <list type="bullet">
/// <item><description>Success and failure states via <see cref="HasError"/> and <see cref="ErrorResult"/></description></item>
/// <item><description>Error and validation error tracking</description></item>
/// <item><description>Typed data payloads</description></item>
/// <item><description>Paging information via the <see cref="Count"/> property</description></item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of the data payload returned in the result.</typeparam>
[ExcludeFromCodeCoverage]
public class Result<T>
{
    /// <summary>
    /// Protected constructor for EF Core / serialization.
    /// Use static factory methods for normal creation.
    /// </summary>
    protected Result() { }

    public bool IsSuccess => this.ErrorResult == ErrorResults.None;
    public ErrorResults ErrorResult { get; set; } = ErrorResults.None;
    public List<string> Errors { get; set; } = [];
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = [];
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int Count { get; set; }

    public static Result<T> Success(T? data = default, int count = 0, string? message = null)
        => new() { Data = data, Count = count, Message = message };

    public static Result<T> Failure(List<string>? errors = null, string? message = null)
        => new() { ErrorResult = ErrorResults.GeneralError, Errors = errors ?? [], Message = message };

    public static Result<T> Failure(string error, string? message = null)
        => Failure([error], message);

    public static Result<T> Failure(Exception exception)
        => Failure([exception?.Message ?? "Unknown error"], exception?.InnerException?.Message);

    public static Result<T> ValidationFailure(Dictionary<string, List<string>>? validationErrors = null, string? message = null, T? data = default)
        => new()
        {
            ErrorResult = ErrorResults.ValidationError,
            ValidationErrors = validationErrors ?? [],
            Message = message,
            Data = data
        };

    public static Result<T> NotFound(string? message = null)
        => new() { ErrorResult = ErrorResults.NotFound, Message = message };
}

/// <summary>
/// Non-generic result wrapper.
/// </summary>
[ExcludeFromCodeCoverage]
public class Result : Result<object?>
{
    protected Result() { }

    public static Result? Success(string? message = null)
        => Success(null, 0, message) as Result;

    public static new Result Failure(List<string>? errors = null, string? message = null)
        => (Result)Result<object?>.Failure(errors, message);

    public static new Result Failure(string error, string? message = null)
        => (Result)Result<object?>.Failure(error, message);

    public static new Result Failure(Exception exception)
        => (Result)Result<object?>.Failure(exception);

    public static Result ValidationFailure(Dictionary<string, List<string>>? validationErrors = null, string? message = null)
        => (Result)Result<object?>.ValidationFailure(validationErrors, message);

    public static new Result NotFound(string? message = null)
        => (Result)Result<object?>.NotFound(message);
}
