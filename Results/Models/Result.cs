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
/// <item><description>Success and failure states via <see cref="IsSuccess"/> and <see cref="ErrorResult"/></description></item>
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
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is protected to enforce the use of factory methods for object creation. 
    /// Use <see cref="Success(T, int, string)"/>, <see cref="Failure(List{string}, string)"/>, 
    /// or other factory methods to create instances.
    /// </remarks>
    protected Result() { }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="ErrorResult"/> is <see cref="ErrorResults.None"/>; otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccess => this.ErrorResult == ErrorResults.None;

    /// <summary>
    /// Gets the type of error that occurred, if any.
    /// </summary>
    /// <value>
    /// An <see cref="ErrorResults"/> value indicating the error type. Defaults to <see cref="ErrorResults.None"/> for successful operations.
    /// </value>
    public ErrorResults ErrorResult { get; set; } = ErrorResults.None;

    /// <summary>
    /// Gets a list of error messages.
    /// </summary>
    /// <value>
    /// A collection of error messages. Empty if the operation was successful or if no errors were recorded.
    /// </value>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets a dictionary of validation errors mapped by field name.
    /// </summary>
    /// <remarks>
    /// This property is used when the operation fails due to validation errors. Each key represents a field name,
    /// and the value is a list of validation error messages for that field.
    /// </remarks>
    /// <value>
    /// A dictionary mapping field names to their validation error messages. Empty if there are no validation errors.
    /// </value>
    public Dictionary<string, List<string>> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Gets an optional message providing additional context about the result.
    /// </summary>
    /// <value>
    /// A message string, or <c>null</c> if no message was provided.
    /// </value>
    public string? Message { get; set; }

    /// <summary>
    /// Gets the data payload returned by the operation.
    /// </summary>
    /// <value>
    /// An instance of type <typeparamref name="T"/>, or <c>default</c> if no data is available.
    /// </value>
    public T? Data { get; set; }

    /// <summary>
    /// Gets the total count of items, typically used for paging scenarios.
    /// </summary>
    /// <remarks>
    /// This property is useful when returning a subset of results and you need to communicate the total number of items available.
    /// </remarks>
    /// <value>
    /// The count of items. Defaults to 0.
    /// </value>
    public int Count { get; set; }

    /// <summary>
    /// Creates a successful result with the specified data.
    /// </summary>
    /// <param name="data">The data payload to include in the result. Defaults to <c>default(<typeparamref name="T"/>)</c>.</param>
    /// <param name="count">The total count of items, typically used for paging. Defaults to 0.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="IsSuccess"/> set to <c>true</c>.</returns>
    public static Result<T> Success(T? data = default, int count = 0, string? message = null)
        => new() { Data = data, Count = count, Message = message };

    /// <summary>
    /// Creates a failed result with the specified error messages.
    /// </summary>
    /// <param name="errors">A list of error messages. If <c>null</c>, an empty list is used.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static Result<T> Failure(List<string>? errors = null, string? message = null)
        => new() { ErrorResult = ErrorResults.GeneralError, Errors = errors ?? [], Message = message };

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static Result<T> Failure(string error, string? message = null)
        => Failure([error], message);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <remarks>
    /// The exception's message is used as the primary error, and the inner exception's message (if present) is used as the additional message.
    /// </remarks>
    /// <param name="exception">The exception to extract error information from.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static Result<T> Failure(Exception exception)
        => Failure([exception?.Message ?? "Unknown error"], exception?.InnerException?.Message);

    /// <summary>
    /// Creates a failed result with validation errors.
    /// </summary>
    /// <remarks>
    /// This factory method is used when an operation fails due to validation errors. The result's <see cref="ErrorResult"/> 
    /// is set to <see cref="ErrorResults.ValidationError"/>, and validation errors are stored in <see cref="ValidationErrors"/>.
    /// </remarks>
    /// <param name="validationErrors">A dictionary mapping field names to lists of validation error messages. If <c>null</c>, an empty dictionary is used.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <param name="data">Optional data to include with the validation failure result. Defaults to <c>default(<typeparamref name="T"/>)</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.ValidationError"/>.</returns>
    public static Result<T> ValidationFailure(Dictionary<string, List<string>>? validationErrors = null, string? message = null, T? data = default)
        => new()
        {
            ErrorResult = ErrorResults.ValidationError,
            ValidationErrors = validationErrors ?? [],
            Message = message,
            Data = data
        };

    /// <summary>
    /// Creates a failed result with a single validation error message.
    /// </summary>
    /// <remarks>
    /// This factory method is used when an operation fails due to a single validation error. The result's <see cref="ErrorResult"/> 
    /// is set to <see cref="ErrorResults.ValidationError"/>, and the error message is stored as a generic validation error.
    /// </remarks>
    /// <param name="validationError">A validation error message.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <param name="data">Optional data to include with the validation failure result. Defaults to <c>default(<typeparamref name="T"/>)</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.ValidationError"/>.</returns>
    public static Result<T> ValidationFailure(string validationError, string? message = null, T? data = default)
        => new()
        {
            ErrorResult = ErrorResults.ValidationError,
            ValidationErrors = new Dictionary<string, List<string>> { { "General", [validationError] } },
            Message = message,
            Data = data
        };

    /// <summary>
    /// Creates a failed result indicating that the requested resource was not found.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.NotFound"/>.</returns>
    public static Result<T> NotFound(string? message = null)
        => new() { ErrorResult = ErrorResults.NotFound, Message = message };

    /// <summary>
    /// Creates a failed result indicating that the operation is unauthorized.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.Unauthorized"/>.</returns>
    public static Result<T> Unauthorized(string? message = null)
        => new() { ErrorResult = ErrorResults.Unauthorized, Message = message };

    /// <summary>
    /// Creates a failed result indicating that the operation is forbidden.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result{T}"/> instance with <see cref="ErrorResult"/> set to <see cref="ErrorResults.Forbidden"/>.</returns>
    public static Result<T> Forbidden(string? message = null)
        => new() { ErrorResult = ErrorResults.Forbidden, Message = message };

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="IsSuccess"/> is <c>false</c>; otherwise, <c>false</c>.
    /// </value>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Returns the data if successful, otherwise throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>The data payload if the operation was successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is not successful.</exception>
    public T GetValueOrThrow()
    {
        if (!IsSuccess)
        {
            var errorMessage = string.IsNullOrEmpty(Message) 
                ? string.Join(", ", this.Errors.Count > 0 ? this.Errors : ["Unknown error"]) 
                : Message;
            throw new InvalidOperationException($"Result failed: {errorMessage}");
        }
        return this.Data!;
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns the provided default value.
    /// </summary>
    /// <param name="defaultValue">The value to return if the operation failed.</param>
    /// <returns>The data payload if successful, otherwise the default value.</returns>
    public T? GetValueOrDefault(T? defaultValue = default)
        => IsSuccess ? this.Data : defaultValue;

    /// <summary>
    /// Sets the data for this result and returns itself for method chaining.
    /// </summary>
    /// <param name="data">The data to set.</param>
    /// <returns>This <see cref="Result{T}"/> instance for method chaining.</returns>
    public Result<T> WithData(T? data)
    {
        this.Data = data;
        return this;
    }

    /// <summary>
    /// Adds an error message to this result. If the result was successful, sets it to <see cref="ErrorResults.GeneralError"/>.
    /// </summary>
    /// <param name="error">The error message to add.</param>
    /// <returns>This <see cref="Result{T}"/> instance for method chaining.</returns>
    public Result<T> AddError(string error)
    {
        if (IsSuccess)
        {
            this.ErrorResult = ErrorResults.GeneralError;
        }
        this.Errors.Add(error);
        return this;
    }

    /// <summary>
    /// Combines this result with another result. If both are successful, returns the first success.
    /// If either is a failure, combines all errors.
    /// </summary>
    /// <param name="other">The result to combine with.</param>
    /// <returns>A new <see cref="Result{T}"/> with combined success/failure state.</returns>
    public Result<T> CombineWith(Result<T> other)
    {
        if (IsSuccess && other.IsSuccess)
            return this;

        var errors = new List<string>();
        if (!IsSuccess)
            errors.AddRange(this.Errors);
        if (!other.IsSuccess)
            errors.AddRange(other.Errors);

        return Failure(errors);
    }

    /// <summary>
    /// Implicitly converts an <see cref="Exception"/> to a failed <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    public static implicit operator Result<T>(Exception exception)
        => Failure(exception);
}

/// <summary>
/// Represents a non-generic result object for operations that do not return typed data.
/// </summary>
/// <remarks>
/// <see cref="Result"/> extends <see cref="Result{T}"/> with <typeparamref name="T"/> set to <see cref="object"/>.
/// Use this class for operations where you only care about success/failure state and error information, 
/// not a specific data payload. All factory methods return <see cref="Result"/> instances instead of 
/// <see cref="Result{T}"/> instances for convenience.
/// </remarks>
[ExcludeFromCodeCoverage]
public class Result : Result<object?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is protected and intended only for serialization / EF Core. Use factory methods for general construction.
    /// </remarks>
    protected Result() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.IsSuccess"/> set to <c>true</c>.</returns>
    public static new Result? Success(string? message = null)
        => Success(null, 0, message) as Result;

    /// <summary>
    /// Creates a failed result with the specified error messages.
    /// </summary>
    /// <param name="errors">A list of error messages. If <c>null</c>, an empty list is used.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static new Result Failure(List<string>? errors = null, string? message = null)
        => (Result)Result<object?>.Failure(errors, message);

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static new Result Failure(string error, string? message = null)
        => (Result)Result<object?>.Failure(error, message);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <remarks>
    /// The exception's message is used as the primary error, and the inner exception's message (if present) is used as the additional message.
    /// </remarks>
    /// <param name="exception">The exception to extract error information from.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.GeneralError"/>.</returns>
    public static new Result Failure(Exception exception)
        => (Result)Result<object?>.Failure(exception);

    /// <summary>
    /// Creates a failed result with validation errors.
    /// </summary>
    /// <remarks>
    /// This factory method is used when an operation fails due to validation errors. The result's <see cref="Result{T}.ErrorResult"/> 
    /// is set to <see cref="ErrorResults.ValidationError"/>, and validation errors are stored in <see cref="Result{T}.ValidationErrors"/>.
    /// </remarks>
    /// <param name="validationErrors">A dictionary mapping field names to lists of validation error messages. If <c>null</c>, an empty dictionary is used.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.ValidationError"/>.</returns>
    public static Result ValidationFailure(Dictionary<string, List<string>>? validationErrors = null, string? message = null)
        => (Result)Result<object?>.ValidationFailure(validationErrors, message);

    /// <summary>
    /// Creates a failed result with a single validation error message.
    /// </summary>
    /// <remarks>
    /// This factory method is used when an operation fails due to a single validation error. The result's <see cref="Result{T}.ErrorResult"/> 
    /// is set to <see cref="ErrorResults.ValidationError"/>, and the error message is stored as a generic validation error.
    /// </remarks>
    /// <param name="validationError">A validation error message.</param>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.ValidationError"/>.</returns>
    public static Result ValidationFailure(string validationError, string? message = null)
        => (Result)Result<object?>.ValidationFailure(validationError, message);

    /// <summary>
    /// Creates a failed result indicating that the requested resource was not found.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.NotFound"/>.</returns>
    public static new Result NotFound(string? message = null)
        => (Result)Result<object?>.NotFound(message);

    /// <summary>
    /// Creates a failed result indicating that the operation is unauthorized.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.Unauthorized"/>.</returns>
    public static new Result Unauthorized(string? message = null)
        => (Result)Result<object?>.Unauthorized(message);

    /// <summary>
    /// Creates a failed result indicating that the operation is forbidden.
    /// </summary>
    /// <param name="message">An optional message providing additional context. Defaults to <c>null</c>.</param>
    /// <returns>A <see cref="Result"/> instance with <see cref="Result{T}.ErrorResult"/> set to <see cref="ErrorResults.Forbidden"/>.</returns>
    public static new Result Forbidden(string? message = null)
        => (Result)Result<object?>.Forbidden(message);
}
