using BusinessLogic.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace BusinessLogic.Helpers;

public class DbSeeder
{
    private readonly Context _context;

    public DbSeeder(Context context)
    {
        _context = context;
    }

    public void Seed()
    {
        //Add permissions
        var permissions = ((IEnumerable<Authorization.Permission>)Enum.GetValues(typeof(Authorization.Permission)))
            .Select(p => new Permission
            {
                Id = (int)p,
                Name = Enum.GetName(p)!
            });

        foreach (var permission in permissions)
        {
            AddIfNotExists(permission, p => p.Name == permission.Name);
        }

        //Seed Admin role
        AddIfNotExists(
            new Role()
            {
                Name = "Admin",
                NormalizedName = "admin",
            },
            r => r.Name == "Admin");

        //Add all permissions to admin role
        var adminRole = _context.Roles.First(r => r.Name == "Admin");
        var rolePermissionIds = _context.Permissions.Select(p => p.Id).ToList();
        foreach (var permissionId in rolePermissionIds)
        {
            AddIfNotExists(
                new RolePermission()
                {
                    PermissionId = permissionId,
                    RoleId = adminRole.Id
                },
                rp => rp.RoleId == adminRole.Id && rp.PermissionId == permissionId);
        }

        //Seed admin user
        AddIfNotExists(
            new User()
            {
                FirstName = "Admin",
                LastName = "Admin",
                Email = "admin@test.org",
                NormalizedEmail = "ADMIN@TEST.ORG",
                PasswordHash = "AQAAAAIAAYagAAAAEPbIfbEpyzBC101sGTPOS6fZvLQCfK85dLDZFWuCf5ngCFkILo8KI9GB3dWY4mHhNg==",//123
                IsEmailPublic = true,
                IsPhoneNumberPublic = true
            },
            u => u.Email == "admin@test.org");

        //Seed admin role for admin user
        var adminUser = _context.Users.First(u => u.Email == "admin@test.org");
        AddIfNotExists(
            new IdentityUserRole<int>()
            {
                RoleId = adminRole.Id,
                UserId = adminUser.Id
            },
            ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

        //Add advertisement categories
        var categories = new List<Category>()
        {
            new()
            {
                Id = 1,
                CanContainAdvertisements = false,
            },
            new()
            {
                Id = 2,
                CanContainAdvertisements = false,
                ParentCategoryId = 1,
            },
            new()
            {
                Id = 3,
                CanContainAdvertisements = false,
                ParentCategoryId = 2,
            },
            new()
            {
                Id = 4,
                CanContainAdvertisements = true,
                ParentCategoryId = 3,
                AdvertisementCount = 1
            },
            new()
            {
                Id = 5,
                CanContainAdvertisements = true,
                ParentCategoryId = 3,
                AdvertisementCount = 2
            }
        };
        foreach(var category in categories)
        {
            AddIfNotExists(category, c => c.Id == category.Id);
        }

        var categoryNameLocales = new List<CategoryNameLocaleText>()
        {
            new()
            {
                Id = 1,
                CategoryId = 1,
                Locale = "ENG",
                Text = "Electric devices"
            },
            new()
            {
                Id = 2,
                CategoryId = 2,
                Locale = "ENG",
                Text = "Phones"
            },
            new()
            {
                Id = 3,
                CategoryId = 3,
                Locale = "ENG",
                Text = "Mobile phones"
            },
            new()
            {
                Id = 4,
                CategoryId = 4,
                Locale = "ENG",
                Text = "Samsung"
            },
            new()
            {
                Id = 5,
                CategoryId = 5,
                Locale = "ENG",
                Text = "Apple"
            },
        };
        foreach(var localeText in categoryNameLocales)
        {
            AddIfNotExists(localeText, t => t.Id == localeText.Id);
        }
    }

    private void AddIfNotExists<T>(T entity, Expression<Func<T, bool>> predicate) where T : class
    {
        if (!_context.Set<T>().Any(predicate))
        {
            _context.Add(entity);
            _context.SaveChanges();
        }
    }
}
