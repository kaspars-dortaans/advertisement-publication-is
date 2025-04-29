using System.Net;

namespace BusinessLogic.Exceptions;

public class ApiException : Exception
{
    public ApiException() { }

    public ApiException(IList<string> errorCodes)
    {
        ErrorCodes = errorCodes;
    }

    public ApiException(IList<string> errorCodes, IDictionary<string, IList<string>> validationErrors)
    {
        ErrorCodes = errorCodes;
        ValidationErrors = validationErrors;
    }

    public IList<string> ErrorCodes { get; set; } = [];
    public IDictionary<string, IList<string>> ValidationErrors { get; set; } = new Dictionary<string, IList<string>>();

    /// <summary>
    /// Add validation error
    /// </summary>
    /// <param name="propertyName"></param> property name which have validation error
    /// <param name="errorMessage"></param> error message
    public void AddValidationError(string propertyName, string errorMessage)
    {
        if (!ValidationErrors.TryGetValue(propertyName, out var errors))
        {
            ValidationErrors.Add(propertyName, [errorMessage] );
        } else
        {
            errors. Add(errorMessage);
        }
    }

    /// <summary>
    /// Set error code which is not validation error
    /// </summary>
    /// <param name="errorCode"></param>
    public void AddErrorCode(string errorCode)
    {
        if (!ErrorCodes.Contains(errorCode))
        {
            ErrorCodes.Add(errorCode);
        }
    }
}
