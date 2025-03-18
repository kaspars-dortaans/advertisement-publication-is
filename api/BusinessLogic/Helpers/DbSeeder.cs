using BusinessLogic.Authorization;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using Attribute = BusinessLogic.Entities.Attribute;

namespace BusinessLogic.Helpers;

public class DbSeeder(Context context, UserManager<User> userManager, RoleManager<Role> roleManager)
{
    private readonly Context _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;

    public async Task Seed()
    {
        //Add permissions
        var permissions = ((IEnumerable<Permissions>)Enum.GetValues(typeof(Permissions)))
            .Select(p => new Permission
            {
                Name = Enum.GetName(p)!
            });
        AddIfNotExistsMultiple(permissions, (pConst) => p => p.Name == pConst.Name);

        //Add roles
        var roles = Enum.GetValues<Roles>();
        foreach (var role in roles)
        {
            var roleName = Enum.GetName(role)!;
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new Role()
                {
                    Name = roleName,
                    NormalizedName = roleName.ToLower()
                });
            }
        }

        var allRoles = _roleManager.Roles.ToList();

        //Add all permissions to admin role
        var adminRole = _context.Roles.First(r => r.Name == nameof(Roles.Admin));
        var adminRolePermissions = _context.Permissions.Select(p => new RolePermission()
        {
            PermissionId = p.Id,
            RoleId = adminRole.Id
        }).ToList();

        AddIfNotExistsMultiple(
            adminRolePermissions,
            pConst => p => pConst.RoleId == p.RoleId && pConst.PermissionId == p.PermissionId);

        //Seed admin user
        var adminEmail = "admin@test.org";
        if ((await _userManager.FindByEmailAsync(adminEmail)) is null)
        {
            var res = await _userManager.CreateAsync(new User()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "Admin",
                Email = adminEmail,
                IsEmailPublic = true,
                IsPhoneNumberPublic = true,
            }, "!23Qwe");

        }

        //Seed roles for dev testing user
        var adminUser = (await _userManager.FindByEmailAsync(adminEmail))!;
        var allRoleNames = _roleManager.Roles.Select(r => r.Name!).ToList();
        await _userManager.AddToRolesAsync(adminUser, allRoleNames);

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

        AddIfNotExistsMultiple(categories, cConst => c => cConst.Id == c.Id);


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

        AddIfNotExistsMultiple(categoryNameLocales, lConst => l => lConst.Id == l.Id);

        // Category attributes
        var attributes = new List<Attribute>()
        {
            new()
            {
                Id = 2,
                Searchable = true,
                Sortable = true,
                ValueType = Enums.ValueTypes.Integer,
                AttributeNameLocales =
                [
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
                ],
                CategoryAttributes =
                [
                    new()
                    {
                        AttributeId = 2,
                        CategoryId = 1
                    }
                ]
            },
            new()
            {
                Id = 3,
                Searchable = false,
                Sortable = false,
                ValueType = Enums.ValueTypes.ValueListEntry,
                AttributeNameLocales =
                [
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
                ],
                CategoryAttributes =
                [
                    new()
                    {
                        AttributeId = 3,
                        CategoryId = 1
                    }
                ],
                AttributeValueList = new AttributeValueList()
                {
                    Id = 1,
                    LocalisedNames =
                    [
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
                    ],
                    ListEntries =
                    [
                        new()
                        {
                            Id = 1,
                            LocalisedNames =
                            [
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
                            ]
                        },
                        new()
                        {
                            Id = 2,
                            LocalisedNames =
                            [
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
                            ]
                        }
                    ]
                }
            }
        };

        AddIfNotExistsMultiple(attributes, aConst => a => aConst.Id == a.Id);

        // Advertisements
        var advertisements = new List<Advertisement>()
        {
            new()
            {
                Id = 1,
                IsActive = true,
                Title = "Advertisement 1",
                AdvertisementText = "Text body",
                CategoryId = 1,
                OwnerId = adminUser.Id,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues =
                [
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
                ]
            },
            new()
            {
                Id = 2,
                IsActive = true,
                Title = "Advertisement 2",
                AdvertisementText = "Text body 2",
                CategoryId = 1,
                OwnerId = adminUser.Id,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues =
                [
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
                ]
            },
        };

        AddIfNotExistsMultiple(advertisements, aConst => a => aConst.Id == a.Id);
    }

    private void AddIfNotExists<T>(T entity, Expression<Func<T, bool>> predicate) where T : class
    {
        if (!_context.Set<T>().Any(predicate))
        {
            _context.Add(entity);
            _context.SaveChanges();
        }
    }

    private void AddIfNotExistsMultiple<T>(IEnumerable<T> entities, Func<T, Expression<Func<T, bool>>> buildPredicate) where T : class
    {
        foreach (var entity in entities)
        {
            AddIfNotExists(entity, buildPredicate(entity));
        }
    }
}
