using System.Text.Json.Serialization;

namespace BusinessLogic.Helpers.CookieSettings;

public class CookieSettingsDto
{
    public string Locale { get; set; } = default!;

    [JsonIgnore]
    public string NormalizedLocale { get { return Locale.ToUpper(); } }
    public string TimeZoneId { get; set; } = default!;
}
