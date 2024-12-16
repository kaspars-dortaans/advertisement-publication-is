using BusinessLogic.Constants;
using BusinessLogic.Dto;
using BusinessLogic.Entities;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
namespace BusinessLogic.Services;

public class UserService(
    UserManager<User> userManager,
    IStorage storage,
    IFilePathResolver filePathResolver,
    IBaseService<Entities.File> fileService) : IUserService
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
    public async Task<RequestResponse> Register(User user, string password, IFormFile? profileImage)
    {
        var createResult = await _userManager.CreateAsync(user, password);

        //Handle user creation errors
        var response = HandleCreateUserResult(createResult);
        if (!response.Successful || profileImage is null)
        {
            return response;
        }

        //Save ProfileImage
        return await SaveProfileImage(user.Email!, profileImage);
    }

    /// <summary>
    /// Handle created user result, return errors if any
    /// </summary>
    /// <param name="createResult"></param>
    /// <returns></returns>
    private static RequestResponse HandleCreateUserResult(IdentityResult createResult)
    {
        var response = new RequestResponse();
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                if (error.Code.StartsWith("Password"))
                {
                    response.AddError("Password", error.Code);
                } else if (error.Code.EndsWith("Email"))
                {
                    response.AddError("Email", error.Code);
                } else if (error.Code.EndsWith("UserName"))
                {
                    response.AddError("UserName", error.Code);
                } else
                {
                    response.AddErrorCode(error.Code);
                }
            }
        }
        return response;
    }

    /// <summary>
    /// Save profile image for user. Adds File entity, updates user profile image reference, saves profile image in storage.
    /// Does not delete previous profile image from storage or it's file entity.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="profileImage"></param>
    /// <returns></returns>
    public async Task<RequestResponse> SaveProfileImage(string userEmail, IFormFile profileImage)
    {
        var response = new RequestResponse();

        //Get created user
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            response.AddErrorCode(CustomErrorCodes.UserNotFound);
            return response;
        }

        //Add file entity
        var filePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, profileImage.FileName);
        var file = await _fileService.AddAsync(new Entities.File()
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

        return response;
    }
}
