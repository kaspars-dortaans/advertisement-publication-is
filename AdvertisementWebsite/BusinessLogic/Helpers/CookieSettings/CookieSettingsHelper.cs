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
    private readonly HttpContext _httpContext;

    public CookieSettingsHelper(IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext ?? throw new ApiException([CustomErrorCodes.InvalidCookie]);
        
        var cookie = _httpContext.Request.Cookies.FirstOrDefault(c => c.Key == CookieConstants.UserSettingCookieName);

        if (string.IsNullOrEmpty(cookie.Value))
        {
            throw new ApiException([CustomErrorCodes.InvalidCookie]);
        }
        var cookieValueString = HttpUtility.UrlDecode(cookie.Value);
        var settingsDto = JsonSerializer.Deserialize<CookieSettingsDto>(cookieValueString, _serializationOptions) 
            ?? throw new ApiException([CustomErrorCodes.InvalidCookie]);
        
        Settings = settingsDto;
    }

    public void AttachToResponse()
    {
        //Cookie encoding is handled by framework
        var cookieStr = JsonSerializer.Serialize(Settings, _serializationOptions);
        _httpContext.Response.Cookies.Append(CookieConstants.UserSettingCookieName, cookieStr, new CookieOptions
        {
            SameSite = CookieConstants.SettingCookieSameSiteMode,
            MaxAge = new TimeSpan(CookieConstants.MaxSettingCookieAgeInDays, 0, 0, 0),
            Secure = CookieConstants.IsSettingCookieSecure
        });
    }
}
