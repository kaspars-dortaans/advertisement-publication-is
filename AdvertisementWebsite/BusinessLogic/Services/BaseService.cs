using BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace BusinessLogic.Services;

public class BaseService<Entity>(Context dbContext) : IBaseService<Entity> where Entity : class
{
    protected DbSet<Entity> DbSet { get; set; } = dbContext.Set<Entity>();
    protected Context DbContext { get; set; } = dbContext;

    public IEnumerable<Entity> GetAllList()
    {
        return [.. DbSet];
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
        await DbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<Entity> UpdateAsync(Entity entity)
    {
        var entityEntry = DbSet.Update(entity);
        await DbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<Entity> RemoveAsync(Entity entity)
    {
        var entityEntry = DbSet.Remove(entity);
        await DbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public Task<Entity?> FirstOrDefaultAsync(Expression<Func<Entity, bool>> predicate)
    {
        return DbSet.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<Entity> Where(Expression<Func<Entity, bool>> predicate)
    {
        return DbSet.Where(predicate);
    }

    public Task DeleteWhereAsync(Expression<Func<Entity, bool>> predicate)
    {
        return DbSet.Where(predicate).DeleteFromQueryAsync();
    }

    public Task UpdateWhereAsync(Expression<Func<Entity, bool>> predicate, Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setters)
    {
        return DbSet.Where(predicate).ExecuteUpdateAsync(setters);
    }

    public async Task<bool> AddIfNotExistsAsync(Entity entity)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {

            var entityExists = await DbSet.ContainsAsync(entity);
            if (!entityExists)
            {
                await DbSet.AddAsync(entity);
            }
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return !entityExists;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public IQueryable<Entity> Include<TProperty>(Expression<Func<Entity, TProperty>> navigationPropertyPath)
    {
        return DbSet.Include(navigationPropertyPath);
    }

    public Task<int> CountAsync(Expression<Func<Entity, bool>> predicate)
    {
        return DbSet.CountAsync(predicate);
    }
}
