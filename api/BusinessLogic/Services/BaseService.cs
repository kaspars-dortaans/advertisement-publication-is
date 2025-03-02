using BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BusinessLogic.Services;

public class BaseService<Entity> : IBaseService<Entity> where Entity : class
{
    protected DbSet<Entity> DbSet { get; set; }
    protected Context DbContext { get; set; }

    public BaseService(Context dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<Entity>();
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
        await DbContext.SaveChangesAsync();
        return entityEntry.Entity;
    }

    public async Task<Entity> UpdateAsync(Entity entity)
    {
        var entityEntry = DbSet.Update(entity);
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
}
