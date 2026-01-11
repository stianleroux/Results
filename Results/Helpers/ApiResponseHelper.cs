namespace Results.Helpers;

using Microsoft.AspNetCore.Mvc;
using Results.Enums;
using Results.Models;

/// <summary>
/// Provides helper methods to translate <see cref="Result"/> or <see cref="Result{T}"/> into appropriate <see cref="ActionResult"/> responses.
/// Maps library result semantics (validation, not found, general error) to HTTP result types.
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    /// Extension group for converting a generic <see cref="Result{T}"/> into an <see cref="ActionResult"/>.
    /// </summary>
    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Converts the current <see cref="Result{T}"/> instance into an <see cref="ActionResult"/>.
        /// Returns 200 (OK) for success, 400 (BadRequest) for validation errors, 404 (NotFound) when not found,
        /// or 500 (Internal Server Error) for other failures.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> representing the appropriate HTTP response.</returns>
        public ActionResult<Result<T>> ToActionResult() => result.IsSuccess
                ? (ActionResult<Result<T>>)new OkObjectResult(result)
                : (ActionResult<Result<T>>)(result.ErrorResult switch
                {
                    ErrorResults.ValidationError => new BadRequestObjectResult(result),
                    ErrorResults.NotFound => new NotFoundObjectResult(result),
                    _ => new ObjectResult(result) { StatusCode = 500 }
                });
    }

    /// <summary>
    /// Extension group for converting a non-generic <see cref="Result"/> into an <see cref="ActionResult"/>.
    /// </summary>
    extension(Result result)
    {
        /// <summary>
        /// Converts the current non-generic <see cref="Result"/> instance into an <see cref="ActionResult"/>.
        /// Returns 200 (OK) for success, 400 (BadRequest) for validation errors, 404 (NotFound) when not found,
        /// or 500 (Internal Server Error) for other failures.
        /// </summary>
        /// <returns>An <see cref="ActionResult"/> representing the appropriate HTTP response.</returns>
        public ActionResult<Result> ToActionResult() => result.IsSuccess
                ? (ActionResult<Result>)new OkObjectResult(result)
                : (ActionResult<Result>)(result.ErrorResult switch
                {
                    ErrorResults.ValidationError => new BadRequestObjectResult(result),
                    ErrorResults.NotFound => new NotFoundObjectResult(result),
                    _ => new ObjectResult(result) { StatusCode = 500 }
                });
    }
}