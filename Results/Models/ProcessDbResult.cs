namespace Results.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class DbResultProcessor<T>
{
    /// <summary>
    /// Converts a database operation into a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="model">The model affected by the operation.</param>
    /// <param name="dbRowsAffected">Number of rows affected.</param>
    /// <param name="errorMessage">Optional custom error message.</param>
    /// <returns>A <see cref="Result{T}"/> representing success or failure.</returns>
    public static Result<T> Outcome(T model, int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected > 0
            ? Result<T>.Success(model)
            : Result<T>.Failure(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? $"Error saving {typeof(T).Name}"
                    : errorMessage.Trim());
}

[ExcludeFromCodeCoverage]
public static class DbResultProcessor
{
    /// <summary>
    /// Converts a database operation into a non-generic <see cref="Result"/>.
    /// </summary>
    /// <param name="dbRowsAffected">Number of rows affected.</param>
    /// <param name="errorMessage">Optional custom error message.</param>
    /// <returns>A <see cref="Result"/> representing success or failure.</returns>
    public static Result? Outcome(int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected > 0
            ? Result.Success()
            : Result.Failure(string.IsNullOrWhiteSpace(errorMessage)
                ? "Database operation failed."
                : errorMessage.Trim());
}