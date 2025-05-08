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
        var existingAttribute = (await DbSet
            .Include(a => a.AttributeNameLocales)
            .FirstOrDefaultAsync(a => a.Id == attribute.Id))
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        if(existingAttribute.AttributeNameLocales != null && existingAttribute.AttributeNameLocales.Count > 0)
        {
            LocalisationHelper.SyncLocaleTexts(existingAttribute.AttributeNameLocales, attribute.AttributeNameLocales);    
        } else
        {
            existingAttribute.AttributeNameLocales = attribute.AttributeNameLocales;
        }
        existingAttribute.AttributeValueListId = attribute.AttributeValueListId;
        existingAttribute.FilterType = attribute.FilterType;
        existingAttribute.ValueType = attribute.ValueType;
        existingAttribute.Searchable = attribute.Searchable;
        existingAttribute.Sortable = attribute.Sortable;
        existingAttribute.ShowOnListItem = attribute.ShowOnListItem;
        existingAttribute.IconName = attribute.IconName;
        existingAttribute.ValueValidationRegex = attribute.ValueValidationRegex;
        
        await UpdateAsync(existingAttribute);
    }

    public async Task<DataTableQueryResponse<AttributeValueList_ListItem>> GetAttributeValueLists(DataTableQuery request)
    {
        var query = DbContext.AttributeValueLists.Select(l => new AttributeValueList_ListItem
        {
            Id = l.Id,
            Title = l.LocalisedNames.First(ln => ln.Locale == _cookieSettingsHelper.Settings.Locale).Text,
            EntryCount = l.ListEntries.Count
        });
        return await DataTableQueryResolver.ResolveDataTableQuery(query, request);
    }

    public async Task UpdateAttributeValueList(AttributeValueList valueList)
    {
        var existingValueList = (await DbContext.AttributeValueLists
            .Include(l => l.LocalisedNames)
            .Include(l => l.ListEntries)
                .ThenInclude(e => e.LocalisedNames)
            .FirstOrDefaultAsync(l => l.Id == valueList.Id))
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        //Update list localized names
        if (existingValueList.LocalisedNames != null && existingValueList.LocalisedNames.Count > 0)
        {
            LocalisationHelper.SyncLocaleTexts(existingValueList.LocalisedNames, valueList.LocalisedNames);
        }
        else
        {
            existingValueList.LocalisedNames = valueList.LocalisedNames;
        }

        //Update list entries
        if (existingValueList.ListEntries != null && existingValueList.ListEntries.Count > 0)
        {
            SyncEntryList(existingValueList.ListEntries, valueList.ListEntries);
        }
        else
        {
            existingValueList.ListEntries = valueList.ListEntries;
        }

        DbContext.AttributeValueLists.Update(existingValueList);
        await DbContext.SaveChangesAsync();

        static void SyncEntryList(ICollection<AttributeValueListEntry> existingEntries, ICollection<AttributeValueListEntry> newEntries)
        {
            var addEntries = new List<AttributeValueListEntry>();
            var updatedEntries = new List<AttributeValueListEntry>();
            foreach (var entry in newEntries)
            {
                if (entry.Id == default)
                {
                    addEntries.Add(entry);
                }
                else
                {
                    updatedEntries.Add(entry);
                }
            }

            foreach (var existingEntry in existingEntries)
            {
                var updatedEntry = updatedEntries.FirstOrDefault(ue => ue.Id == existingEntry.Id);
                if (updatedEntry != null)
                {
                    existingEntry.OrderIndex = updatedEntry.OrderIndex;
                    if (existingEntry.LocalisedNames != null && existingEntry.LocalisedNames.Count > 0)
                    {
                        LocalisationHelper.SyncLocaleTexts(existingEntry.LocalisedNames, updatedEntry.LocalisedNames);
                    }
                    else
                    {
                        existingEntry.LocalisedNames = updatedEntry.LocalisedNames;
                    }
                }
                else
                {
                    existingEntries.Remove(existingEntry);
                }
            }

            foreach (var addEntry in addEntries)
            {
                existingEntries.Add(addEntry);
            }
        }
    }

    public async Task DeleteAttributeValueLists(IEnumerable<int> ids)
    {
        await Where(a => a.AttributeValueListId != null && ids.Any(id => id == a.AttributeValueListId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.AttributeValueListId, a => null));

        await DbContext.AttributeValueLists.Where(vl => ids.Any(id => id == vl.Id)).ExecuteDeleteAsync();
    }
}
