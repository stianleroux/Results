namespace Results.Helpers;

using Microsoft.AspNetCore.Mvc;
using Results.Enums;
using Results.Models;

public static class ApiResponseHelper
{
    public static ActionResult<Result<T>> ResponseOutcome<T>(Result<T> result, ControllerBase controller)
        => result switch
        {
            { ErrorResult: ErrorResults.GeneralError } => controller.StatusCode(500, result),
            { ErrorResult: ErrorResults.ValidationError } => controller.BadRequest(result),
            { ErrorResult: ErrorResults.NotFound } => controller.NotFound(result),
            _ => controller.Ok(result)
        };

    public static ActionResult<Result> ResponseOutcome(Result result, ControllerBase controller)
        => result switch
        {
            { ErrorResult: ErrorResults.GeneralError } => controller.StatusCode(500, result),
            { ErrorResult: ErrorResults.ValidationError } => controller.BadRequest(result),
            { ErrorResult: ErrorResults.NotFound } => controller.NotFound(result),
            _ => controller.Ok(result)
        };
}
