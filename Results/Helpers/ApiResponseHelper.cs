namespace Results.Helpers;

using Microsoft.AspNetCore.Mvc;
using Results.Enums;
using Results.Models;

/// <summary>
/// Provides helper methods to translate <see cref="Result"/> or <see cref="Result{T}"/> into appropriate <see cref="ActionResult"/> responses.
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    /// Maps a <see cref="Result{T}"/> to an appropriate <see cref="ActionResult{T}"/> based on the error result.
    /// </summary>
    /// <typeparam name="T">The type of the result's value.</typeparam>
    /// <param name="result">The result object to process.</param>
    /// <param name="controller">The controller instance used to generate the HTTP response.</param>
    /// <returns>
    /// A corresponding <see cref="ActionResult{T}"/>:
    /// <list type="bullet">
    /// <item><description>500 Internal Server Error if <see cref="ErrorResults.GeneralError"/>.</description></item>
    /// <item><description>400 Bad Request if <see cref="ErrorResults.ValidationError"/>.</description></item>
    /// <item><description>404 Not Found if <see cref="ErrorResults.NotFound"/>.</description></item>
    /// <item><description>200 OK otherwise.</description></item>
    /// </list>
    /// </returns>
    public static ActionResult<Result<T>> ResponseOutcome<T>(Result<T> result, ControllerBase controller)
        => result.Match<T, ActionResult<Result<T>>>(
            onSuccess: _ => controller.Ok(result),
            onFailure: _ => result.ErrorResult switch
            {
                ErrorResults.ValidationError => controller.BadRequest(result),
                ErrorResults.NotFound => controller.NotFound(result),
                _ => controller.StatusCode(500, result)
            });

    /// <summary>
    /// Maps a non-generic <see cref="Result"/> to an appropriate <see cref="ActionResult"/> based on the error result.
    /// </summary>
    /// <param name="result">The result object to process.</param>
    /// <param name="controller">The controller instance used to generate the HTTP response.</param>
    /// <returns>
    /// A corresponding <see cref="ActionResult"/>:
    /// <list type="bullet">
    /// <item><description>500 Internal Server Error if <see cref="ErrorResults.GeneralError"/>.</description></item>
    /// <item><description>400 Bad Request if <see cref="ErrorResults.ValidationError"/>.</description></item>
    /// <item><description>404 Not Found if <see cref="ErrorResults.NotFound"/>.</description></item>
    /// <item><description>200 OK otherwise.</description></item>
    /// </list>
    /// </returns>
    public static ActionResult<Result> ResponseOutcome(Result result, ControllerBase controller)
        => result.Match<ActionResult<Result>>(
            onSuccess: _ => controller.Ok(result),
            onFailure: _ => result.ErrorResult switch
            {
                ErrorResults.ValidationError => controller.BadRequest(result),
                ErrorResults.NotFound => controller.NotFound(result),
                _ => controller.StatusCode(500, result)
            });
}
