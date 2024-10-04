using Microsoft.AspNetCore.Mvc;

namespace Guestbooky.API.Validations;

public static class InvalidModelStateResponseFactory
{
    public static IActionResult DefaultInvalidModelStateResponse(ActionContext context)
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more model validation errors occurred.",
            Detail = "See the errors property for details",
            Instance = context.HttpContext.Request.Path
        };

        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

        var rangeHeaderError = context.ModelState.FirstOrDefault(ms => ms.Key == "Range.Range" && ms.Value.Errors.Count > 0);
        if (rangeHeaderError.Key != null)
        {
            if (rangeHeaderError.Value.Errors.Any(x => x.ErrorMessage.Contains("is not valid for Range.")))
            {
                // Return a 416 status code if a RangeHeaderValue-related error is found
                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status416RequestedRangeNotSatisfiable,
                    ContentTypes = { "application/problem+json" }
                };
            }
            else return null!;
        }
        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }
}
