namespace Results.Helpers;

using Results.Models;

/// <summary>
/// Provides functional-style extensions for working with <see cref="Result{T}"/>.
/// </summary>
public static class FunctionalResult
{
    // ---------- SYNC ----------

    /// <summary>
    /// Transforms the result value if successful.
    /// </summary>
    /// <example>
    /// var result = userResult
    ///     .Bind(user => GetUser(user.Id))
    ///     .Map(m => m.Name);
    /// </example>
    public static Result<T2> Map<T1, T2>(this Result<T1> result, Func<T1, T2> map) =>
        result.HasError
            ? Result<T2>.Failure(result.Errors)
            : Result<T2>.Success(map(result.Data));

    /// <summary>
    /// Chains another result-returning function if successful.
    /// </summary>
    /// <example>
    /// var result = userResult
    ///     .Bind(user => GetUser(user.Id))
    ///     .Map(m => m.Name);
    /// </example>
    public static Result<T2> Bind<T1, T2>(this Result<T1> result, Func<T1, Result<T2>> bind) =>
        result.HasError
            ? Result<T2>.Failure(result.Errors)
            : bind(result.Data);

    /// <summary>
    /// Maps the error list if the result is a failure.
    /// </summary>
    public static Result<T> MapError<T>(this Result<T> result, Func<List<string>, string> map) =>
        result.HasError
            ? Result<T>.Failure(map(result.Errors))
            : Result<T>.Success(result.Data);

    /// <summary>
    /// Executes different logic based on success or failure.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<List<string>, TResult> onFailure) =>
        result.HasError
            ? onFailure(result.Errors)
            : onSuccess(result.Data);

    // ---------- ASYNC ----------

    /// <summary>
    /// Asynchronously transforms the result value if successful.
    /// </summary>
    /// <example>
    /// var result = await GetUserAsync()
    ///     .BindAsync(user => GetUserAsync(user.Id))
    ///     .MapAsync(m => m.Name);
    /// </example>
    public static async Task<Result<T2>> MapAsync<T1, T2>(this Task<Result<T1>> resultTask, Func<T1, T2> map)
    {
        var result = await resultTask;
        return result.Map(map);
    }

    /// <summary>
    /// Asynchronously chains another result-returning function if successful.
    /// </summary>
    /// <example>
    /// var result = await GetUserAsync()
    ///     .BindAsync(user => GetUserAsync(user.Id))
    ///     .MapAsync(m => m.Name);
    /// </example>
    public static async Task<Result<T2>> BindAsync<T1, T2>(this Task<Result<T1>> resultTask, Func<T1, Task<Result<T2>>> bind)
    {
        var result = await resultTask;
        return result.HasError
            ? Result<T2>.Failure(result.Errors)
            : await bind(result.Data);
    }

    /// <summary>
    /// Maps the error list if the result is a failure.
    /// </summary>
    public static Result<T> MapError<T>(this Result<T> result, Func<List<string>, List<string>> map) =>
        result.HasError
            ? Result<T>.Failure(map(result.Errors))
            : Result<T>.Success(result.Data);

    /// <summary>
    /// Asynchronously maps the error list if the result is a failure.
    /// </summary>
    public static async Task<Result<T>> MapErrorAsync<T>(this Task<Result<T>> resultTask, Func<List<string>, List<string>> map)
    {
        var result = await resultTask;
        return result.MapError(map);
    }

    /// <summary>
    /// Asynchronously executes different logic based on success or failure.
    /// </summary>
    /// <example>
    /// var result = await GetUserAsync()
    ///     .BindAsync(user => GetUserAsync(user.Id))
    ///     .MapAsync(m => m.Name)
    ///     .MatchAsync(
    ///         onSuccess: name => Task.FromResult(Ok(name)),
    ///         onFailure: errors => Task.FromResult(BadRequest(errors))
    ///     );
    /// </example>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Task<Result<T>> resultTask,
        Func<T, Task<TResult>> onSuccess,
        Func<List<string>, Task<TResult>> onFailure)
    {
        var result = await resultTask;
        return result.HasError
            ? await onFailure(result.Errors)
            : await onSuccess(result.Data);
    }

    /// <summary>
    /// Executes logic based on success or failure for non-generic <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TResult">The return type of the handlers.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Handler if the result has no error.</param>
    /// <param name="onFailure">Handler if the result has errors.</param>
    /// <returns>The output from the appropriate handler.</returns>
    public static TResult Match<TResult>(
        this Result result,
        Func<string, TResult> onSuccess,
        Func<List<string>, TResult> onFailure) =>
        result.HasError
            ? onFailure(result.Errors ?? [])
            : onSuccess(result.Message);
}