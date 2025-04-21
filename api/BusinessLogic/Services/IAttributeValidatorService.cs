namespace BusinessLogic.Services;

public interface IAttributeValidatorService
{
    public Task ValidateAttributeCategory(int categoryId, string fieldName);
    public Task ValidateAdvertisementAttributeValues(IEnumerable<KeyValuePair<int, string>> attributeValues, int categoryId, string fieldName);
}
