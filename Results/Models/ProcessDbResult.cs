namespace Results.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class DbResultProcessor<T>
{
    /// <summary>
    /// Converts a database operation into a generic Results.Models.Result&lt;T&gt;.
    /// </summary>
    /// <param name="data">The data to return on success.</param>
    /// <param name="dbRowsAffected">Number of rows affected.</param>
    /// <param name="errorMessage">Optional custom error message.</param>
    /// <returns>A Results.Models.Result&lt;T&gt; containing the data if successful, otherwise a failure result.</returns>
    public static Result<T?> Outcome(T? data, int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected > 0
            ? Result<T?>.Success(data)
            : Result<T?>.Failure(string.IsNullOrWhiteSpace(errorMessage) ? "Database operation failed." : errorMessage.Trim());
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
    public static Result Outcome(int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected <= 0
            ? Result.Failure(string.IsNullOrWhiteSpace(errorMessage)
                ? "Database operation failed."
                : errorMessage.Trim())!
            : Result.Success()!;
}