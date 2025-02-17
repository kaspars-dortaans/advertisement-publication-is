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
                ParentCategoryId = 2,
                AdvertisementCount = 1
            },
            new()
            {
                Id = 5,
                CanContainAdvertisements = false,
            },
            new()
            {
                Id = 6,
                ParentCategoryId = 5,
                CanContainAdvertisements = true
            },
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
                Locale = "EN",
                Text = "Electric devices"
            },
            new()
            {
                Id = 2,
                CategoryId = 1,
                Locale = "LV",
                Text = "Elektriskās ierīces"
            },
            new()
            {
                Id = 3,
                CategoryId = 2,
                Locale = "EN",
                Text = "Phones"
            },
            new()
            {
                Id = 4,
                CategoryId = 2,
                Locale = "LV",
                Text = "Tālruņi"
            },
            new()
            {
                Id = 5,
                CategoryId = 3,
                Locale = "EN",
                Text = "Mobile phones"
            },
            new()
            {
                Id = 6,
                CategoryId = 3,
                Locale = "LV",
                Text = "Mobīlie tālruņi"
            },
            new()
            {
                Id = 7,
                CategoryId = 4,
                Locale = "EN",
                Text = "Smartphone"
            },
            new()
            {
                Id = 8,
                CategoryId = 4,
                Locale = "LV",
                Text = "Viedtālruņi"
            },
            new()
            {
                Id = 9,
                CategoryId = 5,
                Locale = "EN",
                Text = "Vehicle"
            },
            new()
            {
                Id = 10,
                CategoryId = 5,
                Locale = "LV",
                Text = "Transportlīdzkļi"
            },
            new()
            {
                Id = 11,
                CategoryId = 6,
                Locale = "EN",
                Text = "Motorcycle"
            },
            new()
            {
                Id = 12,
                CategoryId = 6,
                Locale = "LV",
                Text = "Motocikls"
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
                        Id = 13,
                        AttributeId = 2,
                        Locale = "EN",
                        Text = "Width"
                    },
                    new()
                    {
                        Id = 14,
                        AttributeId = 2,
                        Locale = "LV",
                        Text = "Platums"
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
                        Id = 15,
                        AttributeId = 3,
                        Locale = "EN",
                        Text = "Manufacturer"
                    },
                    new()
                    {
                        Id = 16,
                        AttributeId = 3,
                        Locale = "LV",
                        Text = "Ražotājs"
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
                            Id = 17,
                            Locale = "EN",
                            Text = "Phone manufacturer list",
                            AttributeValueListId = 1
                        },
                        new()
                        {
                            Id = 18,
                            Locale = "LV",
                            Text = "Tālruņu ražotāju saraksts",
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
                                    Id = 19,
                                    Locale = "EN",
                                    Text = "LG"
                                },
                                new()
                                {
                                    Id = 20,
                                    Locale = "LV",
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
                                    Id = 21,
                                    Locale = "EN",
                                    Text = "SAMSUNG"
                                },
                                new()
                                {
                                    Id = 22,
                                    Locale = "LV",
                                    Text = "SAMSUNG"
                                }
                            }
                        }
                    }
                }
            }
        };
        foreach(var attribute in attributes)
        {
            AddIfNotExists(attribute, a => a.Id == attribute.Id);
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
