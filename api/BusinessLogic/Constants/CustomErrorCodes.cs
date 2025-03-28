﻿namespace BusinessLogic.Constants;

public static class CustomErrorCodes
{
    public const string InvalidLoginCredentials = "InvalidLoginCredentials";
    public const string UserWithSuchEmailAlreadyExist = "UserWithSuchEmailAlreadyExist";
    public const string MissingRequired = "Required";
    public const string NotAnEmail = "NotAnEmail";
    public const string NotAPhoneNumber = "NotAPhoneNumber";
    public const string NotAUrl = "NotAUrl";
    public const string PropertyWasNotFound = "PropertyWasNotFound";
    public const string ComparablePropertyDidNotMatch = "ComparablePropertyDidNotMatch";
    public const string InvalidFile = "InvalidFile";
    public const string InvalidFileExtension = "InvalidFileType";
    public const string FileSizeIsTooLarge = "InvalidFileSize";
    public const string DisallowedFileType = "DisallowedFileType";
    public const string InvalidImage = "InvalidImage";
    public const string DisallowedAspectRatio = "DisallowedAspectRatio";
    public const string UserNotFound = "UserNotFound";
    public const string InvalidCookie = "InvalidCookie";
    public const string NotFound = "NotFound";
}
