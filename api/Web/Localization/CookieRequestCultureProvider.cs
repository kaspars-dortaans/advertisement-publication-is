using BusinessLogic.Constants;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers.CookieSettings;
using Microsoft.AspNetCore.Localization;
using System.Text.Json;
using System.Web;

namespace Web.Localization;

public class CookieRequestCultureProvider : IRequestCultureProvider
{
    private static readonly JsonSerializerOptions _serializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var cookie = httpContext.Request.Cookies.FirstOrDefault(c => c.Key == CookieConstants.UserSettingCookieName);

        if (string.IsNullOrEmpty(cookie.Value))
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }
        var cookieValueString = HttpUtility.HtmlDecode(cookie.Value);
        var settingsDto = JsonSerializer.Deserialize<CookieSettingsDto>(cookieValueString, _serializationOptions)
            ?? throw new ApiException([CustomErrorCodes.InvalidCookie]);

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(settingsDto.Locale));
    }
}
