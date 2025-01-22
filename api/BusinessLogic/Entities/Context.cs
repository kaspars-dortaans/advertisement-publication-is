using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Entities;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<User, Role, int>(options)
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
    public virtual DbSet<LocaleText> LocaleTexts { get; set; }
    public virtual DbSet<AttributeNameLocaleText> AttributeNameLocaleTexts { get; set; }
    public virtual DbSet<CategoryNameLocaleText> CategoryNameLocaleTexts { get; set; }
    public virtual DbSet<AttributeValueListLocaleText> AttributeValueListLocaleTexts { get; set; }
    public virtual DbSet<AttributeValueListEntryLocaleText> AttributeValueListLocaleEntryTexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var advertisementBuilder = modelBuilder.Entity<Advertisement>();
        advertisementBuilder
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

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Attributes)
            .WithMany(a => a.UsedInCategories);

        // Functions
        modelBuilder
            .HasDbFunction(() => GetCategoryChildIds(default))
            .HasName("get_child_category_ids");

        modelBuilder.Entity<GetChildCategoryIdsResult>().ToTable("_", t => t.ExcludeFromMigrations());
    }

    /// <summary>
    /// Returns category child category ids (including nested children)
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public IQueryable<GetChildCategoryIdsResult> GetCategoryChildIds(int categoryId) => FromExpression(() => GetCategoryChildIds(categoryId));
}