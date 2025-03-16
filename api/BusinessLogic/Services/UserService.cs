﻿using AutoMapper;
using BusinessLogic.Constants;
using BusinessLogic.Dto.User;
using BusinessLogic.Entities;
using BusinessLogic.Exceptions;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace BusinessLogic.Services;

public class UserService(
    Context context,
    UserManager<User> userManager,
    IStorage storage,
    IFilePathResolver filePathResolver,
    IBaseService<Entities.Files.File> fileService,
    IMapper mapper) : BaseService<User>(context), IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IStorage _storage = storage;
    private readonly IFilePathResolver _filePathResolver = filePathResolver;
    private readonly IBaseService<Entities.Files.File> _fileService = fileService;
    private readonly IMapper _mapper = mapper;

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
                if (error.Code.StartsWith("Password"))
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
    public async Task SaveProfileImage(string userEmail, IFormFile profileImage)
    {
        //TODO: Create thumbnail
        //Get created user
        var user = await _userManager
            .FindByEmailAsync(userEmail)
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        //Add file entity
        var filePath = _filePathResolver.GenerateUniqueFilePath(FileFolderConstants.ProfileImageFolder, profileImage.FileName);
        var file = await _fileService.AddAsync(new Entities.Files.UserImage()
        {
            OwnerUserId = user.Id,
            Path = filePath,
            IsPublic = true
        });

        //Update user profile image reference
        user.ProfileImageFileId = file.Id;
        await _userManager.UpdateAsync(user);

        //Save file data in storage
        var fileReadStream = profileImage.OpenReadStream();
        await _storage.PutFile(filePath, fileReadStream);
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

    public async Task UpdateUserInfo(EditUserInfo info, int id)
    {
        //Update user
        var user = await DbSet
            .Include(u => u.ProfileImageFile)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new ApiException([CustomErrorCodes.UserNotFound]);

        _mapper.Map(info, user);
        var existingProfileImage = user.ProfileImageFile;

        if (info.ProfileImageChanged)
        {
            user.ProfileImageFileId = null;
        }

        HandleIdentityUserResult(await _userManager.UpdateAsync(user));

        if (info.ProfileImageChanged)
        {
            //Delete existing profile image
            if (existingProfileImage is not null)
            {
                if (!string.IsNullOrEmpty(existingProfileImage.Path))
                {
                    await _storage.DeleteFile(existingProfileImage.Path);
                }
                if (!string.IsNullOrEmpty(existingProfileImage.ThumbnailPath))
                {
                    await _storage.DeleteFile(existingProfileImage.ThumbnailPath);
                }
                await _fileService.RemoveAsync(existingProfileImage);
            }

            //Save new profile image
            if (info.ProfileImage is not null)
            {
                await SaveProfileImage(info.Email, info.ProfileImage);
            }
        }

    }
}
