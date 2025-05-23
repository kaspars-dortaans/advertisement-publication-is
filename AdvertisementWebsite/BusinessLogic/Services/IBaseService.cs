﻿using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

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
    public Task<Entity> RemoveAsync(Entity entity);
    public Task<Entity?> FirstOrDefaultAsync(Expression<Func<Entity, bool>> predicate);
    public IQueryable<Entity> Where(Expression<Func<Entity, bool>> predicate);
    public Task DeleteWhereAsync(Expression<Func<Entity, bool>> predicate);
    public Task UpdateWhereAsync(Expression<Func<Entity, bool>> predicate, Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setters);

    /// <summary>
    /// Add entity if it does not exist.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>If entity was added</returns>
    public Task<bool> AddIfNotExistsAsync(Entity entity);
    public IQueryable<Entity> Include<TProperty>(Expression<Func<Entity, TProperty>> navigationPropertyPath);
    public Task<int> CountAsync(Expression<Func<Entity, bool>> predicate);
}
