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
            { Exception: ExistException } => HandleExistException(context),
            { Exception: NotFoundException } => HandleNotFoundException(context),
            { Exception: BadRequestException } => HandleBadRequestException(context),
            { Exception: ForbiddenException } => HandleForbiddenException(context),
            _ => HandleUnhandledException(context)
        };
    }

    private bool HandleUnhandledException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = context.Exception.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status500InternalServerError };

        return true;
    }

    private bool HandleNotFoundException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = context.Exception.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status404NotFound };

        return true;
    }

    private bool HandleForbiddenException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = context.Exception.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status403Forbidden };

        return true;
    }

    private bool HandleExistException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = context.Exception.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status409Conflict };

        return true;
    }

    private bool HandleBadRequestException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = context.Exception.Message,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8"
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status409Conflict };

        return true;
    }
}