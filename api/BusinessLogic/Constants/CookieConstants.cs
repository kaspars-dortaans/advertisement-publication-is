using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Constants;

public class CookieConstants
{
    public const string UserSettingCookieName = "userSettings";
    public const int MaxSettingCookieAgeInDays = 7;
    public const bool IsSettingCookieSecure = true;
    public const SameSiteMode SettingCookieSameSiteMode = SameSiteMode.None;
}
