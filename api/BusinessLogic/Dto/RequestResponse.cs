namespace BusinessLogic.Dto;

public class RequestResponse
{
    /// <summary>
    /// Represents if request was successful
    /// </summary>
    public bool Successful { get; set; } = true;

    public object Result { get; set; } = default!;

    /// <summary>
    /// Custom error codes
    /// </summary>
    public IList<string> ErrorCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Validation errors for specific request dto field, has identical data type and name to <see cref="ValidationProblemDetails" /> Errors property
    /// </summary>
    public IDictionary<string, IList<string>> Errors { get; set; } = new Dictionary<string, IList<string>>();

    public RequestResponse() { }

    public RequestResponse(object result)
    {
        Result = result;
    }

    public RequestResponse(IList<string> errorCodes, IDictionary<string, IList<string>> errors)
    {
        Successful = false;
        ErrorCodes = errorCodes;
        Errors = errors;
    }

    /// <summary>
    /// Set response as unsuccessful and add error
    /// </summary>
    /// <param name="propertyName"></param> property name which have validation error
    /// <param name="errorMessage"></param> error message
    public void AddError(string propertyName, string errorMessage)
    {
        Successful = false;
        if(!Errors.TryGetValue(propertyName, out var errors))
        {
            Errors.Add(propertyName, new List<string> { errorMessage });
        } else
        {
            errors.Add(errorMessage);
        }
    }

    /// <summary>
    /// Set response error code which is not validation error for specific property
    /// </summary>
    /// <param name="errorCode"></param>
    public void AddErrorCode(string errorCode)
    {
        Successful = false;
        ErrorCodes.Add(errorCode);
    }
}
