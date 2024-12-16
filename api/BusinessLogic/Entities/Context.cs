using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Entities;

public class Context : IdentityDbContext<User, Role, int>
{
    public virtual DbSet<Advertisement> Advertisements { get; set; }
    public virtual DbSet<AdvertisementAttributeValue> AdvertisementAttributeValues { get; set; }
    public virtual DbSet<Attribute> Attributes { get; set; }
    public virtual DbSet<AttributeValueList> AttributeValueLists { get; set; }
    public virtual DbSet<AttributeValueListEntry> AttributeValueListEntries { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<File> Files { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
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
            .HasForeignKey<Image>(image => image.AdvertisementId);

        modelBuilder.Entity<Role>()
            .HasMany(r => r.IdentityUserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasOne(user => user.ProfileImageFile)
            .WithOne(file => file.OwnerUser)
            .HasForeignKey<File>(file => file.OwnerUserId);
    }
}
