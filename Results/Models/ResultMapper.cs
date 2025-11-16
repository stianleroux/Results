namespace Results.Models;

using Results.Constants;
using Results.Enums;

public static class ResultMapper
{
    /// <summary>
    /// Maps a <see cref="Result{T1}"/> to a <see cref="Result{T2}"/> using a mapping function.
    /// Preserves errors, validation failures, and not-found results.
    /// </summary>
    public static Result<T2> MapResult<T1, T2>(Result<T1> fromResult, Func<T1, T2> mapper) => fromResult is null
            ? Result<T2>.Failure(ErrorConstants.InternalError)
            : fromResult.ErrorResult switch
            {
                ErrorResults.GeneralError => Result<T2>.Failure(fromResult.Errors, fromResult.Message),
                ErrorResults.NotFound => Result<T2>.NotFound(fromResult.Message),
                ErrorResults.ValidationError => fromResult.Data is not null
                    ? Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message, mapper(fromResult.Data))
                    : Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
                _ => fromResult.Data is not null
                    ? Result<T2>.Success(mapper(fromResult.Data), fromResult.Count, fromResult.Message)
                    : Result<T2>.Success(default, fromResult.Count, fromResult.Message)
            };

    /// <summary>
    /// Converts a generic <see cref="Result{T}"/> into a non-generic <see cref="Result"/>.
    /// Preserves errors, validation failures, and not-found results.
    /// </summary>
    public static Result? MapToEmptyResult<T1>(Result<T1> fromResult) => fromResult is null
            ? Result.Failure(ErrorConstants.InternalError)
            : fromResult.ErrorResult switch
            {
                ErrorResults.GeneralError => Result.Failure(fromResult.Errors, fromResult.Message),
                ErrorResults.NotFound => Result.NotFound(fromResult.Message),
                ErrorResults.ValidationError => Result.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
                _ => Result.Success(fromResult.Message)
            };
}