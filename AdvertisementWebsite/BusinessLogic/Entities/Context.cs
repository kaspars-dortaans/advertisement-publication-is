using BusinessLogic.Entities.DbFunctionDto;
using BusinessLogic.Entities.Files;
using BusinessLogic.Entities.LocaleTexts;
using BusinessLogic.Entities.Payments;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Entities;

public class Context(DbContextOptions<Context> options) : IdentityDbContext<User, Role, int>(options)
{
    public virtual DbSet<Advertisement> Advertisements { get; set; }
    public virtual DbSet<AdvertisementBookmark> AdvertisementBookmarks { get; set; }
    public virtual DbSet<AdvertisementAttributeValue> AdvertisementAttributeValues { get; set; }
    public virtual DbSet<Attribute> Attributes { get; set; }
    public virtual DbSet<AttributeValueList> AttributeValueLists { get; set; }
    public virtual DbSet<AttributeValueListEntry> AttributeValueListEntries { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<CategoryAttribute> CategoryAttributes { get; set; }
    public virtual DbSet<RuleViolationReport> RuleViolationReports { get; set; }
    public virtual DbSet<AdvertisementNotificationSubscription> NotificationSubscriptions { get; set; }
    public virtual DbSet<NotificationSubscriptionAttributeValue> NotificationSubscriptionValues { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Cost> Costs { get; set; }


    //Files and images
    public virtual DbSet<Files.File> Files { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<UserImage> UserImages { get; set; }
    public virtual DbSet<AdvertisementImage> AdvertisementImages { get; set; }
    
    //Locale texts
    public virtual DbSet<LocaleText> LocaleTexts { get; set; }
    public virtual DbSet<AttributeNameLocaleText> AttributeNameLocaleTexts { get; set; }
    public virtual DbSet<CategoryNameLocaleText> CategoryNameLocaleTexts { get; set; }
    public virtual DbSet<AttributeValueListLocaleText> AttributeValueListLocaleTexts { get; set; }
    public virtual DbSet<AttributeValueListEntryLocaleText> AttributeValueListLocaleEntryTexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasMany(r => r.IdentityUserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        var userBuilder = modelBuilder.Entity<User>();
        userBuilder
            .HasOne(user => user.ProfileImageFile)
            .WithOne(file => file.OwnerUser)
            .HasForeignKey<UserImage>(file => file.OwnerUserId);

        userBuilder
            .HasMany(u => u.Chats)
            .WithMany(c => c.Users)
            .UsingEntity<ChatUser>();

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Attributes)
            .WithMany(a => a.UsedInCategories)
            .UsingEntity<CategoryAttribute>(
                r => r.HasOne(ca => ca.Attribute).WithMany(a => a.CategoryAttributes),
                l => l.HasOne(ca => ca.Category).WithMany(c => c.CategoryAttributes),
                j => j.HasKey(ca => new { ca.CategoryId, ca.AttributeId })
            );

        var advertisementBuilder = modelBuilder.Entity<Advertisement>();
        advertisementBuilder
            .HasMany(a => a.BookmarksOwners)
            .WithMany(u => u.BookmarkedAdvertisements)
            .UsingEntity<AdvertisementBookmark>();

        advertisementBuilder
            .HasOne(a => a.Owner)
            .WithMany(u => u.OwnedAdvertisements);

        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.AddedToRoles)
            .UsingEntity<RolePermission>();

        // Functions
        modelBuilder
            .HasDbFunction(() => GetCategoryChildIds(default))
            .HasName("get_child_category_ids");

        modelBuilder
            .HasDbFunction(() => GetCategoryParentIds(default))
            .HasName("get_parent_category_ids");

        modelBuilder.Entity<CategoryIdsResult>().ToTable("_", t => t.ExcludeFromMigrations());
    }

    /// <summary>
    /// Returns category child category ids (including nested children)
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public IQueryable<CategoryIdsResult> GetCategoryChildIds(int categoryId) => FromExpression(() => GetCategoryChildIds(categoryId));

    /// <summary>
    /// Return ids of all parent categories
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    public IQueryable<CategoryIdsResult> GetCategoryParentIds(int categoryId) => FromExpression(() => GetCategoryParentIds(categoryId));
}