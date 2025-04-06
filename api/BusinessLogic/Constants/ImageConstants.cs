namespace BusinessLogic.Constants;

public static class ImageConstants
{
    public const int MaxFileSizeInBytes = 5 * 1024 * 1024; //5MB
    public const string AllowedFileTypes = ".jpg, .jpeg, .png";
    public const double AllowedAspectRatio = 1;
    public const int ThumbnailSize = 200;
    public const string ThumbnailPrefix = "thumbnail-";
    public const int MaxImageCountPerAdvertisement = 10;
}
