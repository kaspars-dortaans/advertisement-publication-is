using BusinessLogic.Constants;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
namespace BusinessLogic.Services;

public class UserService(
    Context context,
    UserManager<User> userManager,
    IStorage storage,
    IFilePathResolver filePathResolver,
    IBaseService<Entities.File> fileService) : BaseService<User>(context), IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;
    private readonly IBaseService<Entities.File> _fileService = fileService;

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="registerDto"></param>
    /// <returns></returns>
    public async Task Register(User user, string password, IFormFile? profileImage)
    {
        var createResult = await _userManager.CreateAsync(user, password);

        //Handle user creation errors
        HandleCreateUserResult(createResult);
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
    private static void HandleCreateUserResult(IdentityResult createResult)
    {
        if (!createResult.Succeeded)
        {
            var apiException = new ApiException();
            foreach (var error in createResult.Errors)
            {
                if (error.Code.StartsWith("Password"))
                {
                    apiException.AddValidationError("Password", error.Code);
                } else if (error.Code.EndsWith("Email"))
                {
                    apiException.AddValidationError("Email", error.Code);
                } else if (error.Code.EndsWith("UserName"))
                {
                    apiException.AddValidationError("UserName", error.Code);
                } else
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
    public async Task SaveProfileImage(string userEmail, IFormFile profileImage)
    {
        //Get created user
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            throw new ApiException([CustomErrorCodes.UserNotFound]);
        }

        //Add file entity
        var filePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, profileImage.FileName);
        var file = await _fileService.AddAsync(new Entities.UserFile()
        {
            OwnerUserId = user.Id,
            Path = filePath
        });

        //Update user profile image reference
        user.ProfileImageFileId = file.Id;
        await _userManager.UpdateAsync(user);

        //Save file data in storage
        var fileReadStream = profileImage.OpenReadStream();
        await _storage.PutFile(filePath, fileReadStream);
    }
}
