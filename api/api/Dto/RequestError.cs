using Microsoft.AspNetCore.Mvc;

namespace api.Dto;

public class RequestError
{
    /// <summary>
    /// Custom error codes
    /// </summary>
    public IEnumerable<string> ErrorCodes { get; set; }

    /// <summary>
    /// Validation errors for specific request dto field, has identical data type and name to <see cref="ValidationProblemDetails" /> Errors property
    /// </summary>
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

    public RequestError(IEnumerable<string> errorCodes, IDictionary<string, string[]> errors)
    {
        ErrorCodes = errorCodes;
        Errors = errors;
    }

    public RequestError(IEnumerable<string> errorCodes)
    {
        ErrorCodes = errorCodes;
    }

    public RequestError(params string[] errorCodes)
    {
        ErrorCodes = errorCodes;
    }
}
