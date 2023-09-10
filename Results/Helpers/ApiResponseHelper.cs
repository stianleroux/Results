namespace Results.Helpers;

using Results.Models;
using Microsoft.AspNetCore.Mvc;

public static class ApiResponseHelper
{
    public static ActionResult<Result<T>> ResponseOutcome<T>(Result<T> result, ControllerBase controller)
        => result switch
        {
            { IsError: true } => controller.StatusCode(500, result),
            { IsValidationError: true } => controller.BadRequest(result),
            { IsNotFound: true } => controller.NotFound(result),
            _ => controller.Ok(result)
        };

    public static ActionResult<CollectionResult<T>> ResponseOutcome<T>(CollectionResult<T> result, ControllerBase controller)
        => result switch
        {
            { IsError: true } => controller.StatusCode(500, result),
            { IsValidationError: true } => controller.BadRequest(result),
            { IsNotFound: true } => controller.NotFound(result),
            _ => controller.Ok(result)
        };

    public static ActionResult<ListResult<T>> ResponseOutcome<T>(ListResult<T> result, ControllerBase controller)
        => result switch
        {
            { IsError: true } => controller.StatusCode(500, result),
            { IsValidationError: true } => controller.BadRequest(result),
            { IsNotFound: true } => controller.NotFound(result),
            _ => controller.Ok(result)
        };

    public static ActionResult<Result> ResponseOutcome(Result result, ControllerBase controller)
        => result switch
        {
            { IsError: true } => controller.StatusCode(500, result),
            { IsValidationError: true } => controller.BadRequest(result),
            { IsNotFound: true } => controller.NotFound(result),
            _ => controller.Ok(result)
        };
}
