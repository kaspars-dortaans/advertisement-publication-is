using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Entities;

public class Context : IdentityDbContext<User, Role, int>
{
    public virtual DbSet<Advertisement> Advertisements { get; set; }
    public virtual DbSet<AdvertisementAttributeValue> AdevertisementAttributeValues { get; set; }
    public virtual DbSet<Attribute> Attributes { get; set; }
    public virtual DbSet<AttributeValueList> AttributeValueLists { get; set; }
    public virtual DbSet<AttributeValueListEntry> AttributeValueListEntries { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Permission> Permisssions { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public Context(DbContextOptions<Context> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Advertisement>()
            .HasOne(advertisement => advertisement.ThumbnailImage)
            .WithOne(image => image.Advertisement)
            .HasForeignKey<Image>(image => image.Advertisementid);
    }
}
