using Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters;

public class CustomExceptionsFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        context.ExceptionHandled = context switch
        {
            { Exception: ExistException } => HandleExistException(context)
        };
    }

    private bool HandleExistException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Request was canceled.",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status409Conflict };

        return true;
    }
}