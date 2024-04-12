using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class BaseService<Entity> : IBaseService<Entity> where Entity : class
    {
        protected DbSet<Entity> DbSet { get; set; }

        public BaseService(DbSet<Entity> set) {
            DbSet = set;
        }

        public IEnumerable<Entity> GetAll()
        {
            return DbSet.ToList();
        }
    }
}
