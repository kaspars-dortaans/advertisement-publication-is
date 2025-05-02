namespace BusinessLogic.Entities.LocaleTexts;

public class LocaleText
{
    public int Id { get; set; }
    public string Locale { get; set; } = default!;
    public string Text { get; set; } = default!;
}
