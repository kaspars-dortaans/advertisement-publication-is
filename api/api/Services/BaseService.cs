using api.Entities;
using Microsoft.EntityFrameworkCore;

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

    public IQueryable<Entity> GetAll()
    {
        return DbSet.AsQueryable();
    }
}
