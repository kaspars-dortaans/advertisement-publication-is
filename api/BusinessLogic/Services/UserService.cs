using BusinessLogic.Constants;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
namespace BusinessLogic.Services;

public class UserService(
    Context context,
    UserManager<User> userManager,
    IStorage storage,
    IFilePathResolver filePathResolver,
    IBaseService<Entities.Files.File> fileService) : BaseService<User>(context), IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;
    private readonly IBaseService<Entities.Files.File> _fileService = fileService;

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    public async Task Register(User user, string password, IFormFile? profileImage, IEnumerable<string>? roles)
    {
        var createResult = await _userManager.CreateAsync(user, password);

        //Handle user creation errors
        HandleIdentityUserResult(createResult);

        if (roles is not null && roles.Any())
        {
            await _userManager.AddToRolesAsync(user, roles);
        }

        if (profileImage is not null)
        {
            //Save ProfileImage
            await SaveProfileImage(user.Email!, profileImage);
        }
    }

    /// <summary>
    /// Handle created user result, return errors if any
    /// </summary>
    /// <param name="createResult"></param>
    /// <returns></returns>
    private static void HandleIdentityUserResult(IdentityResult createResult)
    {
        if (!createResult.Succeeded)
        {
            var apiException = new ApiException();
            foreach (var error in createResult.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    apiException.AddValidationError("CurrentPassword", error.Code);
                }
                else if (error.Code.StartsWith("Password"))
                {
                    apiException.AddValidationError("Password", error.Code);
                }
                else if (error.Code.EndsWith("Email"))
                {
                    apiException.AddValidationError("Email", error.Code);
                }
                else if (error.Code.EndsWith("UserName"))
                {
                    apiException.AddValidationError("UserName", error.Code);
                }
                else
                {
                    apiException.AddErrorCode(error.Code);
                }
            }
            throw apiException;
        }
    }

    /// <summary>
    /// Save profile image for user. Adds File entity, updates user profile image reference, saves profile image in storage.
    /// Does not delete previous profile image from storage or it's file entity.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="profileImage"></param>
    /// <returns></returns>
    public async Task SaveProfileImage(string userEmail, IFormFile? profileImage)
    {
        //Get user
        var user = await DbSet
            .Include(u => u.ProfileImageFile)
            .Where(u => u.Email == userEmail)
            .FirstOrDefaultAsync()
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        //Compute file hash
        var profileImageStream = profileImage?.OpenReadStream();
        string? profileImageHexHash = null;
        if (profileImageStream is not null)
        {
            var sha256 = SHA256.Create();
            var profileImageHash = await sha256.ComputeHashAsync(profileImageStream);
            profileImageHexHash = BitConverter.ToString(profileImageHash).Replace("-", "");
        }

        //If trying to save already existing image return
        if (user.ProfileImageFile?.Hash == profileImageHexHash)
        {
            return;
        }

        //Delete existing profile image, if any
        if (user.ProfileImageFile is not null)
        {
            if (!string.IsNullOrEmpty(user.ProfileImageFile.Path))
            {
                await _storage.DeleteFile(user.ProfileImageFile.Path);
            }
            if (!string.IsNullOrEmpty(user.ProfileImageFile.ThumbnailPath))
            {
                await _storage.DeleteFile(user.ProfileImageFile.ThumbnailPath);
            }
            await _fileService.RemoveAsync(user.ProfileImageFile);
        }

        //If no new profile image to save return
        if (profileImage is null)
        {
            return;
        }

        //Add file entity
        var filePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, profileImage.FileName);
        var thumbnailPath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, ProfileImageConstants.ThumbnailPrefix + profileImage.FileName);
        var file = await _fileService.AddAsync(new Entities.Files.UserImage()
        {
            OwnerUserId = user.Id,
            Path = filePath,
            ThumbnailPath = thumbnailPath,
            IsPublic = true,
            Hash = profileImageHexHash!
        });

        //Update user profile image reference
        user.ProfileImageFileId = file.Id;
        await _userManager.UpdateAsync(user);

        //Make thumbnail
        profileImageStream!.Seek(0, SeekOrigin.Begin);
        var thumbnailStream = await ImageHelper.MakeImageThumbnail(profileImageStream, ProfileImageConstants.ThumbnailSize, ProfileImageConstants.ThumbnailSize);

        //Save files in storage
        profileImageStream.Seek(0, SeekOrigin.Begin);
        thumbnailStream.Seek(0, SeekOrigin.Begin);
        await _storage.PutFiles([
            new (filePath, profileImageStream),
            new (thumbnailPath, thumbnailStream)
        ]);

    }

    public IQueryable<Permission> GetUserPermissions(int userId)
    {
        var userRoleIds = DbContext.UserRoles
            .Where(iur => iur.UserId == userId)
            .Select(iur => iur.RoleId);

        return DbContext.Roles
            .Where(r => userRoleIds.Contains(r.Id))
            .SelectMany(r => r.Permissions)
            .Distinct();
    }

    /// <summary>
    /// If profile image needs to be changed, user entity must have loaded profileImageFile relational entity
    /// </summary>
    /// <param name="user"></param>
    /// <param name="updateProfileImage"></param>
    /// <param name="profileImage"></param>
    /// <returns></returns>
    public async Task UpdateUserInfo(User user, bool updateProfileImage, IFormFile? profileImage)
    {
        HandleIdentityUserResult(await _userManager.UpdateAsync(user));
        if (updateProfileImage)
        {
            await SaveProfileImage(user.Email!, profileImage);
        }
    }

    public async Task ChangePassword(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync("" + userId)
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        var identityResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        HandleIdentityUserResult(identityResult);
    }
}
