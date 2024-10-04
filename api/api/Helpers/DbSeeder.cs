using api.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace api.Helpers;

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
