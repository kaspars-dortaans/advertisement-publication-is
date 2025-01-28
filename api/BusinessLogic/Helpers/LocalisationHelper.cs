using BusinessLogic.Constants;
using BusinessLogic.Entities;

namespace BusinessLogic.Helpers;

public static class LocalisationHelper
{
    public static string Localise<T> (this ICollection<T> localisations, string locale) where T: LocaleText
    {
        return localisations.First(lt => lt.Locale == locale || lt.Locale == LocalisationConstants.TextNotLocalised).Text;
    } 
}
