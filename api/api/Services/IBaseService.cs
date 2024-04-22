namespace api.Services;

public interface IBaseService<Entity> where Entity : class
{
    public IEnumerable<Entity> GetAllList();
    public IQueryable<Entity> GetAll();
}
