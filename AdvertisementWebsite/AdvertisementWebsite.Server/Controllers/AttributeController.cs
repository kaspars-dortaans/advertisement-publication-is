using AdvertisementWebsite.Server.Dto.Attributes;
using AutoMapper;
using BusinessLogic.Authorization;
using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Dto.Attribute;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;
using BusinessLogic.Entities.LocaleTexts;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvertisementWebsite.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class AttributeController(
    IAttributeService attributeService,
    IBaseService<AttributeValueList> attributeValueListService,
    IMapper mapper,
    CookieSettingsHelper cookieSettingsHelper
    ) : ControllerBase
{
    private readonly IAttributeService _attributeService = attributeService;
    private readonly IBaseService<AttributeValueList> _attributeValueListService = attributeValueListService;
    private readonly IMapper _mapper = mapper;
    private readonly CookieSettingsHelper _cookieSettingsHelper = cookieSettingsHelper;

    [HasPermission(Permissions.ViewAllAttributes)]
    [ProducesResponseType<DataTableQueryResponse<AttributeListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<AttributeListItem>> GetAttributes(DataTableQuery query)
    {
        return await _attributeService.GetAttributes(query);
    }

    [HasPermission(Permissions.CreateAttribute)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateAttribute(PutAttributeRequest request)
    {
        var newAttribute = _mapper.Map<BusinessLogic.Entities.Attribute>(request);
        await _attributeService.AddAsync(newAttribute);
    }

    [HasPermission(Permissions.ViewAllAttributes)]
    [ProducesResponseType<PutAttributeRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<PutAttributeRequest> GetAttributeFormInfo(int attributeId)
    {
        var attributeData = (await _attributeService
            .Where(a => a.Id == attributeId)
            .Select(a => new PutAttributeRequest
            {
                Id = a.Id,
                ValueType = a.ValueType,
                FilterType = a.FilterType,
                ValueValidationRegex = a.ValueValidationRegex,
                AttributeValueListId = a.AttributeValueListId,
                AttributeValueListName = a.AttributeValueList != null
                    ? a.AttributeValueList.LocalisedNames.Localise(_cookieSettingsHelper.Settings.Locale)
                    : null,
                Sortable = a.Sortable,
                Searchable = a.Searchable,
                ShowOnListItem = a.ShowOnListItem,
                LocalizedNames = a.AttributeNameLocales.Select(l => (KeyValuePair<string, string>?)new KeyValuePair<string, string>(l.Locale, l.Text)),
                IconName = a.IconName
            })
            .FirstOrDefaultAsync())
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        return attributeData;
    }

    [HasPermission(Permissions.ViewAllAttributes)]
    [ProducesResponseType<IEnumerable<KeyValuePair<int, string>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IEnumerable<KeyValuePair<int, string>>> GetAttributeValueListLookup()
    {
        return await _attributeValueListService
            .GetAll()
            .Select(l => new KeyValuePair<int, string>(l.Id, l.LocalisedNames.Localise(_cookieSettingsHelper.Settings.Locale)))
            .ToListAsync();
    }

    [HasPermission(Permissions.EditAttribute)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditAttribute(PutAttributeRequest request)
    {
        if (request.Id == null)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>() {
                { nameof(PutAttributeRequest.Id), [CustomErrorCodes.NotFound]}
            });
        }
        var updatedAttribute = _mapper.Map<BusinessLogic.Entities.Attribute>(request);
        await _attributeService.UpdateAttribute(updatedAttribute);
    }

    [HasPermission(Permissions.DeleteAttribute)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteAttribute(IEnumerable<int> ids)
    {
        await _attributeService.DeleteWhereAsync(c => ids.Contains(c.Id));
    }

    [HasPermission(Permissions.ViewAllAttributeValueLists)]
    [ProducesResponseType<DataTableQueryResponse<AttributeValueList_ListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<DataTableQueryResponse<AttributeValueList_ListItem>> GetAttributeValueLists(DataTableQuery request)
    {
        return await _attributeService.GetAttributeValueLists(request);
    }

    [HasPermission(Permissions.CreateAttributeValueList)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task CreateAttributeValueList(PutAttributeValueListRequest request)
    {
        var valueList = new AttributeValueList
        {
            LocalisedNames = request.LocalizedNames.Where(ln => ln != null).Select(ln => new AttributeValueListLocaleText
            {
                Locale = ln!.Value.Key,
                Text = ln.Value.Value ?? string.Empty
            }).ToList(),
            ListEntries = request.Entries.Select(e => new AttributeValueListEntry
            {
                OrderIndex = e.OrderIndex,
                LocalisedNames = e.LocalizedNames.Select(ln => new AttributeValueListEntryLocaleText
                {
                    Locale = ln!.Value.Key,
                    Text = ln.Value.Value ?? string.Empty
                }).ToList()
            }).ToList()
        };
        await _attributeValueListService.AddAsync(valueList);
    }

    [HasPermission(Permissions.ViewAllAttributeValueLists)]
    [ProducesResponseType<PutAttributeValueListRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<PutAttributeValueListRequest> GetAttributeValueListFormInfo(int valueListId)
    {
        var formInfo = (await _attributeValueListService.Where(vl => vl.Id == valueListId)
            .Select(vl => new PutAttributeValueListRequest
            {
                Id = vl.Id,
                LocalizedNames = vl.LocalisedNames.Select(ln => (KeyValuePair<string, string>?)new KeyValuePair<string, string>(ln.Locale, ln.Text)),
                Entries = vl.ListEntries.Select(e => new AttributeValueListEntryDto
                {
                    Id = e.Id,
                    OrderIndex = e.OrderIndex,
                    LocalizedNames = e.LocalisedNames.Select(ln => (KeyValuePair<string, string>?)new KeyValuePair<string, string>(ln.Locale, ln.Text))
                }).OrderBy(e => e.OrderIndex).ToList()
            })
            .FirstOrDefaultAsync())
            ?? throw new ApiException([CustomErrorCodes.NotFound]);

        return formInfo;
    }

    [HasPermission(Permissions.EditAttributeValueList)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task EditAttributeValueList(PutAttributeValueListRequest request)
    {
        if (!request.Id.HasValue)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { nameof(PutAttributeValueListRequest.Id), [CustomErrorCodes.MissingRequired] }
            });
        }

        var valueList = new AttributeValueList
        {
            Id = request.Id.Value,
            LocalisedNames = request.LocalizedNames.Where(ln => ln != null).Select(ln => new AttributeValueListLocaleText
            {
                Locale = ln!.Value.Key,
                Text = ln.Value.Value ?? string.Empty
            }).ToList(),
            ListEntries = request.Entries.Select(e => new AttributeValueListEntry
            {
                Id = e.Id ?? default,
                OrderIndex = e.OrderIndex,
                LocalisedNames = e.LocalizedNames.Where(ln => ln != null).Select(ln => new AttributeValueListEntryLocaleText
                {
                    AttributeValueListEntryId = request.Id.Value,
                    Locale = ln!.Value.Key,
                    Text = ln.Value.Value ?? string.Empty
                }).ToList()
            }).ToList()
        };
        await _attributeService.UpdateAttributeValueList(valueList);
    }

    [HasPermission(Permissions.DeleteAttributeValueList)]
    [ProducesResponseType<OkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<RequestExceptionResponse>(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task DeleteAttributeValueLists(IEnumerable<int> ids)
    {

        await _attributeService.DeleteAttributeValueLists(ids);
    }
}
