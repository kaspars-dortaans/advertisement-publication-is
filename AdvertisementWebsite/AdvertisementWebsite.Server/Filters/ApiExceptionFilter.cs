using BusinessLogic.Dto;
using BusinessLogic.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdvertisementWebsite.Server.Filters;

public class ApiExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    void IActionFilter.OnActionExecuting(ActionExecutingContext context)
    { }

    void IActionFilter.OnActionExecuted(ActionExecutedContext context)
    { 
        if(context.Exception is ApiException validationException)
        {
            var exceptionResult = new RequestExceptionResponse(validationException.ErrorCodes, validationException.ValidationErrors);
            context.Result = new BadRequestObjectResult(exceptionResult);
            context.ExceptionHandled = true;
        }
    }
}
