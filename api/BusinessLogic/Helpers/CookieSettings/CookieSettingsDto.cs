namespace BusinessLogic.Helpers.CookieSettings;

public class CookieSettingsDto
{
    public string Locale { get; set; } = default!;
    public string NormalizedLocale { get { return Locale.ToUpper(); } }
    public string TimeZoneId { get; set; } = default!;
}
