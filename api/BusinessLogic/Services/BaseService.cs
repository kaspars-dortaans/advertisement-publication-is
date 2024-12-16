using BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BusinessLogic.Services;

public class BaseService<Entity> : IBaseService<Entity> where Entity : class
{
    protected DbSet<Entity> DbSet { get; set; }
    protected Context _dbContext { get; set; }

    public BaseService(Context context)
    {
        _dbContext = context;
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
    {
        return await DbSet.AnyAsync(predicate);
    }
    public async Task<Entity> AddAsync(Entity entity)
    {
        var entityEntry = DbSet.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<Entity> UpdateAsync(Entity entity)
    {
        var entityEntry = DbSet.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }
}
