using BusinessLogic.Constants;
using BusinessLogic.Entities.LocaleTexts;

namespace BusinessLogic.Helpers;

public static class LocalisationHelper
{
    public static string Localise<T> (this ICollection<T> localisations, string locale) where T: LocaleText
    {
        return localisations.First(lt => lt.Locale == locale || lt.Locale == LocalisationConstants.TextNotLocalised).Text;
    }

    public static void SyncLocaleTexts<T>(ICollection<T> existingLocaleTexts, ICollection<T> newLocaleTexts) where T : LocaleText
    {
        foreach (var existingLocaleText in existingLocaleTexts)
        {

            var updatedLocaleText = newLocaleTexts.FirstOrDefault(ult => ult.Locale == existingLocaleText.Locale);
            if (updatedLocaleText != null)
            {
                //Update
                existingLocaleText.Text = updatedLocaleText.Text;
            }
            else
            {
                //Delete
                existingLocaleTexts.Remove(existingLocaleText);
            }
        }

        //Add new
        foreach (var newText in newLocaleTexts)
        {
            if (!existingLocaleTexts.Any(elt => elt.Locale == newText.Locale))
                existingLocaleTexts.Add(newText);
        }
    }
}
