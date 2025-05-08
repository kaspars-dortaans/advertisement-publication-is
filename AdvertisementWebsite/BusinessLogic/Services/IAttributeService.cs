using BusinessLogic.Dto.Attribute;
using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Entities;

namespace BusinessLogic.Services;

public interface IAttributeService : IBaseService<Entities.Attribute>
{
    public Task<DataTableQueryResponse<AttributeListItem>> GetAttributes(DataTableQuery query);
    public Task UpdateAttribute(Entities.Attribute attribute);
    public Task<DataTableQueryResponse<AttributeValueList_ListItem>> GetAttributeValueLists(DataTableQuery request);
    public Task UpdateAttributeValueList(AttributeValueList valueList);
    public Task DeleteAttributeValueLists(IEnumerable<int> ids);
}
