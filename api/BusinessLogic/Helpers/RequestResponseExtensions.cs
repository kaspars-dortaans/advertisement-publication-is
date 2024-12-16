using BusinessLogic.Dto;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogic.Helpers;

public static class RequestResponseExtensions
{
    public static IActionResult ToObjectResult(this RequestResponse response)
    {
        if (response.Successful)
        {
            return new OkObjectResult(response);
        } else
        {
            return new BadRequestObjectResult(response);
        }
    }
}
