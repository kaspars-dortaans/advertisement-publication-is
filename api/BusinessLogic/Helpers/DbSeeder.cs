using BusinessLogic.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using Attribute = BusinessLogic.Entities.Attribute;

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
            },
            new()
            {
                Id = 6,
                CanContainAdvertisements = false
            },
            new()
            {
                Id = 7,
                CanContainAdvertisements = true,
                ParentCategoryId = 6,
                AdvertisementCount = 2
            }
        };
        foreach(var category in categories)
        {
            AddIfNotExists(category, c => c.Id == category.Id);
        }

        //Add category names
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
            new()
            {
                Id = 6,
                CategoryId = 6,
                Locale = "ENG",
                Text = "Vehicle"
            },
            new()
            {
                Id = 7,
                CategoryId = 7,
                Locale = "ENG",
                Text = "Bike"
            },
        };
        foreach(var localeText in categoryNameLocales)
        {
            AddIfNotExists(localeText, t => t.Id == localeText.Id);
        }

        // Category attributes
        var attributes = new List<Attribute>()
        {
            new()
            {
                Id = 2,
                Searchable = true,
                Sortable = true,
                ValueType = Enums.ValueTypes.Integer,
                AttributeNameLocales = new List<AttributeNameLocaleText>()
                {
                    new()
                    {
                        Id = 9,
                        AttributeId = 2,
                        Locale = "ENG",
                        Text = "Width"
                    }
                },
                CategoryAttributes = new List<CategoryAttribute>()
                {
                    new()
                    {
                        AttributeId = 2,
                        CategoryId = 1
                    }
                }
            },
            new()
            {
                Id = 3,
                Searchable = false,
                Sortable = false,
                ValueType = Enums.ValueTypes.ValueListEntry,
                AttributeNameLocales = new List<AttributeNameLocaleText>()
                {
                    new()
                    {
                        Id = 10,
                        AttributeId = 3,
                        Locale = "ENG",
                        Text = "Manufacturer"
                    }
                },
                CategoryAttributes = new List<CategoryAttribute>()
                {
                    new()
                    {
                        AttributeId = 3,
                        CategoryId = 1
                    }
                },
                AttributeValueList = new AttributeValueList()
                {
                    Id = 1,
                    LocalisedNames = new List<AttributeValueListLocaleText>()
                    {
                        new()
                        {
                            Id = 11,
                            Locale = "ENG",
                            Text = "Manufacturer value list",
                            AttributeValueListId = 1
                        }
                    },
                    ListEntries = new List<AttributeValueListEntry>()
                    {
                        new()
                        {
                            Id = 1,
                            LocalisedNames = new List<AttributeValueListEntryLocaleText>()
                            {
                                new()
                                {
                                    Id = 12,
                                    Locale = "ENG",
                                    Text = "LG"
                                },
                            }
                        },
                        new()
                        {
                            Id = 2,
                            LocalisedNames = new List<AttributeValueListEntryLocaleText>()
                            {
                                new()
                                {
                                    Id = 13,
                                    Locale = "ENG",
                                    Text = "SAMSUNG"
                                }
                            }
                        }
                    }
                }
            }
        };
        foreach(var attribtue in attributes)
        {
            AddIfNotExists(attribtue, a => a.Id == attribtue.Id);
        }

        // Advertisements
        var advertisements = new List<Advertisement>()
        {
            new()
            {
                Id = 1,
                Title = "Advertisement 1",
                AdvertisementText = "Text body",
                CategoryId = 1,
                OwnerId = 1,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues = new List<AdvertisementAttributeValue>()
                {
                    new()
                    {
                        Id = 1,
                        AttributeId = 2,
                        Value = "12"
                    },
                    new()
                    {
                        Id = 2,
                        AttributeId = 3,
                        Value = "1"
                    }
                }
            },
            new()
            {
                Id = 2,
                Title = "Advertisement 2",
                AdvertisementText = "Text body 2",
                CategoryId = 1,
                OwnerId = 1,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues = new List<AdvertisementAttributeValue>()
                {
                    new()
                    {
                        Id = 3,
                        AttributeId = 2,
                        Value = "10"
                    },
                    new()
                    {
                        Id = 4,
                        AttributeId = 3,
                        Value = "2"
                    }
                }
            },
        };
        foreach(var advertisement in advertisements)
        {
            AddIfNotExists(advertisement, a => a.Id == advertisement.Id);
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
