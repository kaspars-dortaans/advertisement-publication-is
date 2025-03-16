namespace BusinessLogic.Constants;

public static class ProfileImageConstants
{
    public const int MaxFileSizeInBytes = 3 * 8 * 1024 * 1024; //3MB
    public const string AllowedFileTypes = ".jpg, .png";
    public const double AllowedAspectRatio = 1;
    public const int ThumbnailSize = 200;
    public const string ThumbnailPrefix = "thumbnail-";
}
