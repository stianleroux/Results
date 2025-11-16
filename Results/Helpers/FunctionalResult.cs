namespace Results.Helpers;

using Results.Models;

/// <summary>
/// Provides functional-style extensions for working with <see cref="Result{T}"/>.
/// Contains synchronous and asynchronous helpers for mapping, binding, error mapping and matching.
/// </summary>
public static class FunctionalResult
{
    // ---------- Result<T> (sync) ----------

    /// <summary>
    /// Extension group for synchronous operations on <see cref="Result{T1}"/> values.
    /// </summary>
    extension<T1>(Result<T1> result)
    {
        /// <summary>
        /// Maps the successful <see cref="Result{T1}"/> value to a new value of type <typeparamref name="T2"/>.
        /// If the source result contains errors the errors are propagated and a failure result is returned.
        /// </summary>
        /// <typeparam name="T2">The target type of the mapping function.</typeparam>
        /// <param name="map">Mapping function applied when the result is successful.</param>
        /// <returns>
        /// A <see cref="Result{T2}"/> containing the mapped value when <paramref name="result"/> is successful,
        /// otherwise a failure result containing the same errors.
        /// </returns>
        public Result<T2> Map<T2>(Func<T1, T2> map)
            => result.IsSuccess
                ? Result<T2>.Success(map(result.Data))
                : Result<T2>.Failure(result.Errors);

        /// <summary>
        /// Binds (flatMaps) the successful value using a function that returns another <see cref="Result{T2}"/>.
        /// Propagates errors if the source result is a failure.
        /// </summary>
        /// <typeparam name="T2">The target result value type.</typeparam>
        /// <param name="bind">A function that produces a <see cref="Result{T2}"/> from the successful value.</param>
        /// <returns>
        /// The <see cref="Result{T2}"/> returned by <paramref name="bind"/> when <paramref name="result"/> is successful,
        /// otherwise a failure result with the original errors.
        /// </returns>
        public Result<T2> Bind<T2>(Func<T1, Result<T2>> bind)
            => result.IsSuccess
                ? bind(result.Data)
                : Result<T2>.Failure(result.Errors);

        /// <summary>
        /// Maps the error list to a single error string and returns a failure result with that single error.
        /// If the source result is successful the original success is returned.
        /// </summary>
        /// <param name="map">Function that converts the existing error list into a single error string.</param>
        /// <returns>
        /// A successful <see cref="Result{T1}"/> when <paramref name="result"/> is successful,
        /// otherwise a failure result containing the mapped single error.
        /// </returns>
        public Result<T1> MapError(Func<List<string>, string> map)
            => !result.IsSuccess
                ? Result<T1>.Success(result.Data)
                : Result<T1>.Failure([map(result.Errors)]);

        /// <summary>
        /// Maps the list of errors to a new list of error strings.
        /// If the source result is successful the original success is returned.
        /// </summary>
        /// <param name="map">Function that converts the existing error list into a new error list.</param>
        /// <returns>
        /// A successful <see cref="Result{T1}"/> when <paramref name="result"/> is successful,
        /// otherwise a failure result containing the mapped error list.
        /// </returns>
        public Result<T1> MapError(Func<List<string>, List<string>> map)
            => result.IsSuccess
                ? Result<T1>.Success(result.Data)
                : Result<T1>.Failure(map(result.Errors));

        /// <summary>
        /// Matches on the result: executes <paramref name="onSuccess"/> when the result is successful
        /// or <paramref name="onFailure"/> when the result contains errors.
        /// </summary>
        /// <typeparam name="TResult">The return type of the match functions.</typeparam>
        /// <param name="onSuccess">Function to execute for a successful result.</param>
        /// <param name="onFailure">Function to execute for a failure result.</param>
        /// <returns>The result of the executed match function.</returns>
        public TResult Match<TResult>(
            Func<T1, TResult> onSuccess,
            Func<List<string>, TResult> onFailure) => result.IsSuccess
                ? onSuccess(result.Data)
                : onFailure(result.Errors);
    }

    // ---------- Result<T> (async) ----------

    /// <summary>
    /// Extension group for asynchronous operations on <see cref="Result{T1}"/> wrapped in a <see cref="Task"/>.
    /// </summary>
    extension<T1>(Task<Result<T1>> resultTask)
    {
        /// <summary>
        /// Asynchronously maps the successful result value to a new type.
        /// </summary>
        /// <typeparam name="T2">The target type of the mapping function.</typeparam>
        /// <param name="map">Mapping function applied when the awaited result is successful.</param>
        /// <returns>A task containing a <see cref="Result{T2}"/> with the mapped value or the original errors.</returns>
        public async Task<Result<T2>> MapAsync<T2>(Func<T1, T2> map)
        {
            var result = await resultTask;
            return result.Map(map);
        }

        /// <summary>
        /// Asynchronously binds (flatMaps) the successful value using a function that returns a <see cref="Task{Result{T2}}"/>.
        /// </summary>
        /// <typeparam name="T2">The target result value type.</typeparam>
        /// <param name="bind">A function that produces a <see cref="Task{Result{T2}}"/> from the successful value.</param>
        /// <returns>A task containing the <see cref="Result{T2}"/> produced by <paramref name="bind"/> or the original errors.</returns>
        public async Task<Result<T2>> BindAsync<T2>(Func<T1, Task<Result<T2>>> bind)
        {
            var result = await resultTask;
            return result.IsSuccess
                ? await bind(result.Data)
                : Result<T2>.Failure(result.Errors);
        }

        /// <summary>
        /// Asynchronously maps the error list to a new list of error strings.
        /// </summary>
        /// <param name="map">Function that converts the existing error list into a new error list.</param>
        /// <returns>A task containing a <see cref="Result{T1}"/> with the mapped errors or the original success.</returns>
        public async Task<Result<T1>> MapErrorAsync(Func<List<string>, List<string>> map)
        {
            var result = await resultTask;
            return result.MapError(map);
        }

        /// <summary>
        /// Asynchronously matches on the awaited result: executes <paramref name="onSuccess"/> or <paramref name="onFailure"/> accordingly.
        /// </summary>
        /// <typeparam name="TResult">The return type of the match functions.</typeparam>
        /// <param name="onSuccess">Function to execute for a successful result.</param>
        /// <param name="onFailure">Function to execute for a failure result.</param>
        /// <returns>A task containing the result of the executed match function.</returns>
        public async Task<TResult> MatchAsync<TResult>(
            Func<T1, Task<TResult>> onSuccess,
            Func<List<string>, Task<TResult>> onFailure)
        {
            var result = await resultTask;

            return result.IsSuccess
                ? await onSuccess(result.Data)
                : await onFailure(result.Errors);
        }
    }

    // ---------- Non-generic Result ----------

    /// <summary>
    /// Extension group for non-generic <see cref="Result"/> values.
    /// </summary>
    extension(Result result)
    {
        /// <summary>
        /// Matches on the non-generic result, executing <paramref name="onSuccess"/> if successful
        /// or <paramref name="onFailure"/> when it contains errors.
        /// </summary>
        /// <typeparam name="TResult">The return type of the match functions.</typeparam>
        /// <param name="onSuccess">Function to execute for a successful result (receives the success message).</param>
        /// <param name="onFailure">Function to execute for a failure result (receives the list of errors).</param>
        /// <returns>The result of the executed match function.</returns>
        public TResult Match<TResult>(
            Func<string, TResult> onSuccess,
            Func<List<string>, TResult> onFailure) => result.IsSuccess
                ? onSuccess(result.Message)
                : onFailure(result.Errors ?? []);
    }
}