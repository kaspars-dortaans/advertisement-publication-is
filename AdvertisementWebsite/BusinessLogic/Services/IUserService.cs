using BusinessLogic.Dto.DataTableQuery;
using BusinessLogic.Dto.Users;
using BusinessLogic.Entities;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services;

public interface IUserService : IBaseService<User>
{
    public Task Register(User user, string password, IFormFile? profilePicture, IEnumerable<string>? roles);
    public IQueryable<Permission> GetUserPermissions(int userId);
    public Task UpdateUser(User user, bool updateProfileImage, IFormFile? profileImage, IEnumerable<string>? roles = null);
    public Task ChangePassword(int userId, string currentPassword, string newPassword);
    public Task<IEnumerable<string>> GetAllRoleNames();
    public Task<DataTableQueryResponse<UserListItem>> GetUserList(DataTableQuery request);
    public Task DeleteUsers(IEnumerable<int> ids);
}
