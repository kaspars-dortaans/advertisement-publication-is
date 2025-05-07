using BusinessLogic.Constants;
using BusinessLogic.Dto.Attribute;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services;

public class AttributeService(
    Context dbContext,
    CookieSettingsHelper cookieSettingsHelper
    ) : BaseService<Entities.Attribute>(dbContext), IAttributeService
{
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    public async Task<DataTableQueryResponse<AttributeListItem>> GetAttributes(DataTableQuery query)
    {
        var attributes = GetAll()
            .Select(a => new AttributeListItem
            {
                Id = a.Id,
                Title = a.AttributeNameLocales.First(l => l.Locale == _cookieSettingsHelper.Settings.Locale).Text,
                ValueType = a.ValueType,
                FilterType = a.FilterType,
                AttributeValueListName = a.AttributeValueList != null
                    ? a.AttributeValueList.LocalisedNames.First(lt => lt.Locale == _cookieSettingsHelper.Settings.Locale).Text
                    : null,
                Sortable = a.Sortable,
                Searchable = a.Searchable,
                IconName = a.IconName,
                ShowOnListItem = a.ShowOnListItem
            });
        var result = await DataTableQueryResolver.ResolveDataTableQuery(attributes, query);
        return result;
    }

    public async Task UpdateAttribute(Entities.Attribute attribute)
    {
        var existingAttribute = (await Where(a => a.Id == attribute.Id)
            .AsNoTracking()
            .Select(a => new { a.AttributeNameLocales })
            .FirstOrDefaultAsync())
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        if (attribute.AttributeNameLocales != null && existingAttribute.AttributeNameLocales != null)
        {
            foreach (var newNameLocale in attribute.AttributeNameLocales)
            {
                var existingNameLocale = existingAttribute.AttributeNameLocales
                    .FirstOrDefault(ln => ln.Locale == newNameLocale.Locale);

                if (existingNameLocale != null)
                {
                    newNameLocale.Id = existingNameLocale.Id;
                }
            }
        }

        await UpdateAsync(attribute);
    }
}
