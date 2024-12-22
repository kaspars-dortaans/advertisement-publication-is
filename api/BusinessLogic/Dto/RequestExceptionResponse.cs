using Microsoft.AspNetCore.Mvc;

namespace BusinessLogic.Dto;

public class RequestExceptionResponse : ValidationProblemDetails
{
    /// <summary>
    /// Custom error codes
    /// </summary>
    public IList<string> ErrorCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Validation errors for specific request dto field, has identical data type and name to <see cref="ValidationProblemDetails" /> Errors property
    /// </summary>

    public RequestExceptionResponse(IList<string> errorCodes, IDictionary<string, string[]> errors)
    {
        ErrorCodes = errorCodes;
        Errors = errors;
    }

    public RequestExceptionResponse(IList<string> errorCodes, IDictionary<string, IList<string>> errors)
    {
        ErrorCodes = errorCodes;
        Errors = errors.Select(x => new KeyValuePair<string, string[]>(x.Key, x.Value.ToArray())).ToDictionary();
    }
}
