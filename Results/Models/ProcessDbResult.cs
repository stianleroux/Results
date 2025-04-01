namespace Results.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public static class DbResultProcessor<T>
{
    public static Result<T> Outcome(T model, int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected > 0
            ? Result<T>.Success(model)
            : Result<T>.Failure(string.IsNullOrWhiteSpace(errorMessage)
                ? $"Error saving {typeof(T).Name}"
                : errorMessage.Trim());
}

[ExcludeFromCodeCoverage]
public static class EFResultProcessor
{
    public static Result Outcome(int dbRowsAffected, string? errorMessage = null)
        => dbRowsAffected > 0
            ? Result.Success()
            : Result.Failure(errorMessage?.Trim() ?? "Database operation failed.");
}
