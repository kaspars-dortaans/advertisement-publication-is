using BusinessLogic.Dto.Attribute;
using BusinessLogic.Dto.DataTableQuery;

namespace BusinessLogic.Services;

public interface IAttributeService : IBaseService<Entities.Attribute>
{
    public Task<DataTableQueryResponse<AttributeListItem>> GetAttributes(DataTableQuery query);
    public Task UpdateAttribute(Entities.Attribute attribute);
}
