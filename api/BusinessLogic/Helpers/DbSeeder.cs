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
                CanContainAdvertisements = false,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Electric devices"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Elektriskās ierīces"
                    },
                ]
            },
            new() {
                CanContainAdvertisements = false,
                ParentCategoryId = 1,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Phones"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Tālruņi"
                    },
                ]
            },
            new()
            {
                CanContainAdvertisements = true,
                ParentCategoryId = 2,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Mobile phones"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Mobīlie tālruņi"
                    },
                ]
            },
            new()
            {
                CanContainAdvertisements = true,
                ParentCategoryId = 2,
                AdvertisementCount = 1,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Smartphone"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Viedtālruņi"
                    },
                ]
            },
            new()
            {
                CanContainAdvertisements = false,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Vehicle"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Transportlīdzkļi"
                    },
                ]
            },
            new()
            {
                ParentCategoryId = 5,
                CanContainAdvertisements = true,
                LocalisedNames = [
                    new() {
                        Locale = "EN",
                        Text = "Motorcycle"
                    },
                    new() {
                        Locale = "LV",
                        Text = "Motocikls"
                    },
                ]
            },
        };

        AddIfNotExistsMultiple(categories, cConst => (c) => cConst.LocalisedNames.ElementAt(0).Text == c.LocalisedNames.Where(l => l.Locale == "EN").First().Text);

        // Category attributes
        var attributes = new List<Attribute>()
        {
            new()
            {
                Searchable = true,
                Sortable = false,
                ValueType = Enums.ValueTypes.Integer,
                AttributeNameLocales =
                [
                    new()
                    {
                        Locale = "EN",
                        Text = "Width"
                    },
                    new()
                    {
                        Locale = "LV",
                        Text = "Platums"
                    }
                ],
                CategoryAttributes =
                [
                    new()
                    {
                        AttributeId = 1,
                        CategoryId = 3
                    }
                ]
            },
            new()
            {
                Searchable = false,
                Sortable = true,
                ValueType = Enums.ValueTypes.ValueListEntry,
                AttributeNameLocales =
                [
                    new()
                    {
                        Locale = "EN",
                        Text = "Manufacturer"
                    },
                    new()
                    {
                        Locale = "LV",
                        Text = "Ražotājs"
                    }
                ],
                CategoryAttributes =
                [
                    new()
                    {
                        AttributeId = 2,
                        CategoryId = 3
                    }
                ],
                AttributeValueList = new AttributeValueList()
                {
                    LocalisedNames =
                    [
                        new()
                        {
                            Locale = "EN",
                            Text = "Phone manufacturer list",
                        },
                        new()
                        {
                            Locale = "LV",
                            Text = "Tālruņu ražotāju saraksts",
                        }
                    ],
                    ListEntries =
                    [
                        new()
                        {
                            LocalisedNames =
                            [
                                new()
                                {
                                    Locale = "EN",
                                    Text = "LG"
                                },
                                new()
                                {
                                    Locale = "LV",
                                    Text = "LG"
                                },
                            ]
                        },
                        new()
                        {
                            LocalisedNames =
                            [
                                new()
                                {
                                    Locale = "EN",
                                    Text = "SAMSUNG"
                                },
                                new()
                                {
                                    Locale = "LV",
                                    Text = "SAMSUNG"
                                }
                            ]
                        }
                    ]
                }
            }
        };

        AddIfNotExistsMultiple(attributes, aConst => a => aConst.AttributeNameLocales.ElementAt(0).Text == a.AttributeNameLocales.Where(l => l.Locale == "EN").First().Text);

        // Advertisements
        var advertisements = new List<Advertisement>()
        {
            new()
            {
                IsActive = true,
                Title = "Advertisement 1",
                AdvertisementText = "Text body",
                CategoryId = 4,
                OwnerId = adminUser.Id,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues =
                [
                    new()
                    {
                        AttributeId = 1,
                        Value = "12"
                    },
                    new()
                    {
                        AttributeId = 2,
                        Value = "1"
                    }
                ]
            },
            new()
            {
                IsActive = true,
                Title = "Advertisement 2",
                AdvertisementText = "Text body 2",
                CategoryId = 4,
                OwnerId = adminUser.Id,
                PostedDate = DateTime.UtcNow,
                ValidToDate = DateTime.UtcNow.AddMonths(1),
                ViewCount = 0,
                AttributeValues =
                [
                    new()
                    {
                        AttributeId = 1,
                        Value = "10"
                    },
                    new()
                    {
                        AttributeId = 2,
                        Value = "2"
                    }
                ]
            },
        };

        AddIfNotExistsMultiple(advertisements, aConst => a => aConst.Title == a.Title);
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
