namespace Results.Models;

using Results.Constants;

public static class ResultMapper
{
    public static Result<T2> MapResult<T1, T2>(Result<T1> fromResult, Func<T1, T2> mapper)
        => fromResult switch
        {
            null => Result<T2>.Failure(ErrorConstants.InternalError),
            { IsError: true } => Result<T2>.Failure(fromResult.Errors, fromResult.Message),
            { IsNotFound: true } => Result<T2>.NotFound(fromResult.Message),
            { IsValidationError: true, Data: null } => Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
            { IsValidationError: true, Data: not null } => Result<T2>.ValidationFailure(fromResult.ValidationErrors, fromResult.Message, mapper(fromResult.Data)),
            { Data: not null } => Result<T2>.Success(mapper(fromResult.Data), fromResult.Message),
            _ => Result<T2>.Success(default, fromResult.Message),
        };

    public static Result MapToEmptyResult<T1>(Result<T1> fromResult)
        => fromResult switch
        {
            null => Result.Failure(ErrorConstants.InternalError),
            { IsError: true } => Result.Failure(fromResult.Errors, fromResult.Message),
            { IsNotFound: true } => Result.NotFound(fromResult.Message),
            { IsValidationError: true } => Result.ValidationFailure(fromResult.ValidationErrors, fromResult.Message),
            _ => Result.Success(fromResult.Message)
        };
}
