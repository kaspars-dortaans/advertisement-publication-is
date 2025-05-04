using BusinessLogic.Authorization;
using BusinessLogic.Entities;
using BusinessLogic.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Attribute = BusinessLogic.Entities.Attribute;

namespace BusinessLogic.Helpers;

public class DbSeeder(Context context, UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<DbSeeder> logger)
{
    private readonly Context _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly ILogger _logger = logger;

    public async Task Seed()
    {
        try
        {
            await TrySeed();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during database seeding: {message}", ex.Message);
        }
    }

    private async Task TrySeed()
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

        var adminRole = _context.Roles.First(r => r.Name == Enum.GetName(Roles.Admin));
        var defaultAdminPermissionList = new List<Permissions>() {
            //Manage own profile
            Permissions.ViewOwnProfileInfo,
            Permissions.EditOwnProfileInfo,
            Permissions.ChangeOwnPassword,

            //Manage own advertisement bookmarks
            Permissions.ViewAdvertisementBookmarks,
            Permissions.BookmarkAdvertisement,
            Permissions.EditAdvertisementBookmark,

            //Messages
            Permissions.ViewMessages,
            Permissions.SendMessage,

            //Payment
            Permissions.ViewSystemPayments,

            //Manage categories
            Permissions.CreateCategory,
            Permissions.EditCategory,
            Permissions.DeleteCategory,

            //Manage attributes
            Permissions.ViewAllAttributes,
            Permissions.CreateAttribute,
            Permissions.EditAttribute,
            Permissions.DeleteAttribute,

            //Manage users
            Permissions.ViewAllUsers,
            Permissions.CreateUser,
            Permissions.EditAnyUser,
            Permissions.DeleteAnyUser,

            //Manage roles
            Permissions.ViewAllRoles,
            Permissions.AddRole,
            Permissions.EditRole,
            Permissions.DeleteRole,

            //Manage permissions
            Permissions.ViewAllPermissions,
            Permissions.AddPermission,
            Permissions.EditPermission,
            Permissions.DeletePermission,

            //Manage advertisements
            Permissions.CreateAdvertisement,
            Permissions.ViewAllAdvertisements,
            Permissions.EditAnyAdvertisement,
            Permissions.DeleteAnyAdvertisement,

            //Manage advertisement notification subscriptions
            Permissions.CreateAdvertisementNotificationSubscription,
            Permissions.ViewAllAdvertisementNotificationSubscriptions,
            Permissions.EditAnyAdvertisementNotificationSubscription,
            Permissions.DeleteAnyAdvertisementNotificationSubscription,

            //Rule violation reports
            Permissions.ViewRuleViolationReports,
            Permissions.ResolveRuleViolationReport,
        }.Select(p => Enum.GetName(p)).ToList();

        var adminRolePermissions = _context.Permissions
            .Where(p => defaultAdminPermissionList.Any(permissionName => permissionName == p.Name))
            .Select(p => new RolePermission()
            {
                PermissionId = p.Id,
                RoleId = adminRole.Id
            }).ToList();

        AddIfNotExistsMultiple(
            adminRolePermissions,
            pConst => p => pConst.RoleId == p.RoleId && pConst.PermissionId == p.PermissionId);

        var userRole = _context.Roles.First(r => r.Name == Enum.GetName(Roles.User));
        var defaultUserRolePermissionList = new List<Permissions>()
        {
            //Manage own profile
            Permissions.ViewOwnProfileInfo,
            Permissions.EditOwnProfileInfo,
            Permissions.ChangeOwnPassword,

            //Manage own advertisement bookmarks
            Permissions.ViewAdvertisementBookmarks,
            Permissions.BookmarkAdvertisement,
            Permissions.EditAdvertisementBookmark,

            //Manage own advertisements
            Permissions.CreateOwnedAdvertisement,
            Permissions.ViewOwnedAdvertisements,
            Permissions.EditOwnedAdvertisement,
            Permissions.DeleteOwnedAdvertisement,

            //Messages
            Permissions.ViewMessages,
            Permissions.SendMessage,

            //Manage own advertisement notification subscriptions
            Permissions.ViewOwnedAdvertisementNotificationSubscriptions,
            Permissions.CreateOwnedAdvertisementNotificationSubscription,
            Permissions.EditOwnedAdvertisementNotificationSubscriptions,
            Permissions.DeleteOwnedAdvertisementNotificationSubscriptions,

            //Payment
            Permissions.ViewOwnPayments,
            Permissions.MakePayment
        }.Select(p => Enum.GetName(p)).ToList();

        var userRolePermissions = _context.Permissions
            .Where(p => defaultUserRolePermissionList.Any(permissionName => permissionName == p.Name))
            .Select(p => new RolePermission()
            {
                PermissionId = p.Id,
                RoleId = userRole.Id
            }).ToList();

        AddIfNotExistsMultiple(
            userRolePermissions,
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

        //Seed roles for dev admin user
        var adminUser = (await _userManager.FindByEmailAsync(adminEmail))!;
        await _userManager.AddToRolesAsync(adminUser, [Enum.GetName(Roles.Admin)!]);

        //Seed regular user
        var userEmail = "user@test.org";
        if ((await _userManager.FindByEmailAsync(userEmail)) is null)
        {
            var res = await _userManager.CreateAsync(new User()
            {
                FirstName = "Test",
                LastName = "Test",
                UserName = "Test",
                Email = userEmail,
                IsEmailPublic = true,
                IsPhoneNumberPublic = true,
            }, "!23Qwe");

        }

        //Seed roles for dev regular user
        var user = (await _userManager.FindByEmailAsync(userEmail))!;
        await _userManager.AddToRolesAsync(user, [Enum.GetName(Roles.User)!]);

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
                ValueType = ValueTypes.Integer,
                ShowOnListItem = true,
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
                        CategoryId = 2
                    }
                ]
            },
            new()
            {
                Searchable = false,
                Sortable = true,
                ValueType = ValueTypes.ValueListEntry,
                ShowOnListItem = true,
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
                        CategoryId = 2
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
                CreatedDate = DateTime.UtcNow,
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
                CreatedDate = DateTime.UtcNow,
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

        //Costs
        List<Cost> costs = [
            new(){
                Type = CostType.CreateAdvertisement,
                Amount = 1
            },
            new(){
                Type = CostType.AdvertisementPerDay,
                Amount = 0.05m
            },
            new(){
                Type = CostType.CreateAdvertisementNotificationSubscription,
                Amount = 2
            },
            new(){
                Type = CostType.SubscriptionPerDay,
                Amount = 0.1m
            }
        ];

        AddIfNotExistsMultiple(costs, cConst => c => cConst.Type == c.Type);
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
