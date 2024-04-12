using api.Entities;
using Microsoft.AspNetCore.Identity;

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
        //Seed Admin role
        if (!_context.Roles.Any(r => r.Name == "Admin")){
            _context.Roles.Add(new Role()
            {
                Name = "Admin",
                NormalizedName = "admin",
            });
            _context.SaveChanges();
        }

        //Seed admin user
        if(!_context.Users.Any(u => u.NormalizedUserName == "admin")){
            _context.Users.Add(new User()
            {
                FirstName = "Admin",
                LastName = "Admin",
                NormalizedUserName = "admin",

            });
            _context.SaveChanges();
        }

        //Seed admin role for admin user
        var adminUser = _context.Users.First(u => u.NormalizedUserName == "admin");
        var adminRole = _context.Roles.First(r => r.Name == "Admin");
        if(!_context.UserRoles.Any(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id)){
            _context.Add(new IdentityUserRole<int>()
            {
                RoleId = adminRole.Id,
                UserId = adminUser.Id
            });
            _context.SaveChanges();
        }
    }
}
