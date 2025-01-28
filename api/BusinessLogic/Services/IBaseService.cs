using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace BusinessLogic.Services;

public interface IBaseService<Entity> where Entity : class
{
    public IEnumerable<Entity> GetAllList();
    public Task<IEnumerable<Entity>> GetAllListAsync();
    public IQueryable<Entity> GetAll();
    public bool Exists(Expression<Func<Entity, bool>> predicate);
    public Task<bool> ExistsAsync(Expression<Func<Entity, bool>> predicate);
    public Task<Entity> AddAsync(Entity entity);
    public Task<Entity> UpdateAsync(Entity entity);
    public IQueryable<Entity> Where(Expression<Func<Entity, bool>> predicate);
}
