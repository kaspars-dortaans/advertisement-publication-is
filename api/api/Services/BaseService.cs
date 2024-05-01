using api.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace api.Services;

public class BaseService<Entity> : IBaseService<Entity> where Entity : class
{
    protected DbSet<Entity> DbSet { get; set; }

    public BaseService(Context context) {
        DbSet = context.Set<Entity>();
    }

    public IEnumerable<Entity> GetAllList()
    {
        return DbSet.ToList();
    }

    public async Task<IEnumerable<Entity>> GetAllListAsync()
    {
        return await DbSet.ToListAsync();
    }

    public IQueryable<Entity> GetAll()
    {
        return DbSet.AsQueryable();
    }

    public bool Exists(Expression<Func<Entity, bool>> predicate)
    {
        return DbSet.Any(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Entity, bool>> predicate)
    {return await DbSet.AnyAsync(predicate);

    }
}
