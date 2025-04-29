using BusinessLogic.Constants;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BusinessLogic.Services;

public class AttributeValidatorService(Context DbContext) : IAttributeValidatorService
{
    /// <summary>
    /// Validate if category can contain advertisements
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public async Task ValidateAttributeCategory(int categoryId, string fieldName)
    {
        var canAddToCategory = await DbContext.Categories.AnyAsync(c => c.Id == categoryId && c.CanContainAdvertisements);
        if (!canAddToCategory)
        {
            throw new ApiException([], new Dictionary<string, IList<string>>
            {
                { fieldName, [CustomErrorCodes.CategoryCanNotContainAdvertisements] }
            });
        }
    }

    /// <summary>
    /// Validate if advertisement attribute values are valid
    /// </summary>
    /// <param name="attributeValues"></param>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    public async Task ValidateAdvertisementAttributeValues(IEnumerable<KeyValuePair<int, string>> attributeValues, int categoryId, string fieldName)
    {
        var submittedAttributeIds = attributeValues.Select(av => av.Key).ToList();
        var parentCategoryIds = DbContext.GetCategoryParentIds(categoryId);
        var attributes = await DbContext.Attributes
            .Where(a => a.UsedInCategories.Any(c => c.Id == categoryId || parentCategoryIds.Any(parentCategory => parentCategory.Id == c.Id)) && submittedAttributeIds.Contains(a.Id))
            .Select(a => new
            {
                a.Id,
                a.ValueType,
                a.ValueValidationRegex,
                ValueListEntryIds = a.AttributeValueList != null ? a.AttributeValueList.ListEntries.Select(e => e.Id.ToString()) : null
            }).ToListAsync();

        List<KeyValuePair<int, string>> invalidAttributes = [];
        var attributeValueArray = attributeValues.ToArray();
        for (var i = 0; i < attributeValueArray.Length; i++)
        {
            var attribute = attributes.FirstOrDefault(a => a.Id == attributeValueArray[i].Key);
            if (attribute is null)
            {
                continue;
            }

            var value = attributeValueArray[i].Value;
            switch (attribute.ValueType)
            {
                case Enums.ValueTypes.ValueListEntry:
                    if (attribute.ValueListEntryIds is not null && attribute.ValueListEntryIds.All(id => id != value))
                    {
                        invalidAttributes.Add(new(i, CustomErrorCodes.OptionNotFound));
                        continue;
                    }
                    break;

                case Enums.ValueTypes.Integer:
                    if (!int.TryParse(value, out int _))
                    {
                        invalidAttributes.Add(new(i, CustomErrorCodes.ValueMustBeInteger));
                        continue;
                    }
                    break;

                case Enums.ValueTypes.Decimal:
                    if (!double.TryParse(value, out double _))
                    {
                        invalidAttributes.Add(new(i, CustomErrorCodes.ValueMustBeNumber));
                        continue;
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(attribute.ValueValidationRegex))
            {
                //Validate with regex if present
                if (!Regex.IsMatch(attributeValueArray[i].Value, attribute.ValueValidationRegex))
                {
                    invalidAttributes.Add(new(i, CustomErrorCodes.InvalidValue));
                    continue;
                }
            }
        };

        if (invalidAttributes.Count != 0)
        {
            var validationErrors = invalidAttributes
                .GroupBy(ia => ia.Key)
                .ToDictionary(
                    g => fieldName + "[" + g.Key + "]",
                    g => (IList<string>)g.Select(ia => ia.Value).ToList());

            throw new ApiException([], validationErrors);
        }
    }

}
