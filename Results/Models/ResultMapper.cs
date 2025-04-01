namespace Results.Models;

using Results.Constants;
using Results.Enums;

public static class ResultMapper
{
    public static Result<T2> MapResult<T1, T2>(Result<T1> fromResult, Func<T1, T2> mapper)
        => fromResult switch
        {
            null => Result<T2>.Failure(ErrorConstants.InternalError),
            { ErrorResult: ErrorResults.GeneralError } => Result<T2>.Failure(fromResult.Errors, fromResult.Message),
            { ErrorResult: ErrorResults.NotFound } => Result<T2>.NotFound(fromResult.Message),
            { ErrorResult: ErrorResults.ValidationError, Data: null } => Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
            { ErrorResult: ErrorResults.ValidationError, Data: not null } => Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message, mapper(fromResult.Data)),
            { Data: not null } => Result<T2>.Success(mapper(fromResult.Data), fromResult.Count, fromResult.Message),
            _ => Result<T2>.Success(default, fromResult.Count, fromResult.Message),
        };

    public static Result MapToEmptyResult<T1>(Result<T1> fromResult)
        => fromResult switch
        {
            null => Result.Failure(ErrorConstants.InternalError),
            { ErrorResult: ErrorResults.GeneralError } => Result.Failure(fromResult.Errors, fromResult.Message),
            { ErrorResult: ErrorResults.NotFound } => Result.NotFound(fromResult.Message),
            { ErrorResult: ErrorResults.ValidationError } => Result.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
            _ => Result.Success(fromResult.Message)
        };
}
