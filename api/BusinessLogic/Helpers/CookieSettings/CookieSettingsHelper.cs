using BusinessLogic.Constants;
using BusinessLogic.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Web;

namespace BusinessLogic.Helpers.CookieSettings;

public class CookieSettingsHelper
{
    public readonly CookieSettingsDto Settings;
    private static readonly JsonSerializerOptions _serializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CookieSettingsHelper(IHttpContextAccessor contextAccessor)
    {
        var context = contextAccessor.HttpContext;
        if (context is null)
        {
            throw new ApiException([CustomErrorCodes.InvalidCookie]);
        }

        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == CookieConstants.UserSettingCookieName);

        if (string.IsNullOrEmpty(cookie.Value))
        {
            throw new ApiException([CustomErrorCodes.InvalidCookie]);
        }
        var cookieValueString = HttpUtility.HtmlDecode(cookie.Value);
        var settingsDto = JsonSerializer.Deserialize<CookieSettingsDto>(cookieValueString, _serializationOptions) 
            ?? throw new ApiException([CustomErrorCodes.InvalidCookie]);
        
        Settings = settingsDto;
    }
}
