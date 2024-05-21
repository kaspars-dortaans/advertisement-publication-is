using System.Linq.Expressions;

namespace api.Services;

public interface IBaseService<Entity> where Entity : class
{
    public IEnumerable<Entity> GetAllList();
    public Task<IEnumerable<Entity>> GetAllListAsync();
    public IQueryable<Entity> GetAll();
    public bool Exists(Expression<Func<Entity, bool>> predicate);
    public Task<bool> ExistsAsync(Expression<Func<Entity, bool>> predicate);
}
