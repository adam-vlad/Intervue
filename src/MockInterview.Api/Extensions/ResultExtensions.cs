using Microsoft.AspNetCore.Mvc;
using MockInterview.Application.Common;

namespace MockInterview.Api.Extensions;

/// <summary>
/// Extension method that maps Result&lt;T&gt; to proper HTTP responses.
/// ErrorKind → HTTP status: Validation→400, NotFound→404, Conflict→409, Failure→500.
/// SuccessKind → HTTP status: Ok→200, Created→201, NoContent→204.
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.SuccessKind switch
            {
                SuccessKind.Created => new ObjectResult(result.Value) { StatusCode = 201 },
                SuccessKind.NoContent => new NoContentResult(),
                _ => new OkObjectResult(result.Value)
            };
        }

        // Get the first error's kind to determine the HTTP status code
        var errorKind = result.Errors.First().Kind;

        var statusCode = errorKind switch
        {
            ErrorKind.Validation => 400,
            ErrorKind.NotFound => 404,
            ErrorKind.Conflict => 409,
            _ => 500
        };

        // Return a ProblemDetails response (standard HTTP error format)
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errorKind.ToString(),
            Detail = string.Join("; ", result.Errors.Select(e => e.Message)),
            Extensions =
            {
                ["errors"] = result.Errors.Select(e => new { e.Code, e.Message }).ToList()
            }
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
