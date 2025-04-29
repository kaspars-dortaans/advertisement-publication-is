using BusinessLogic.Constants;
using BusinessLogic.Helpers.Storage;
using ImageMagick;

namespace BusinessLogic.Helpers;

public class ImageHelper(IStorage storage)
{
    private readonly IStorage _storage = storage;

    public static async Task<Stream> MakeImageThumbnail(Stream imageStream, uint width, uint height)
    {
        imageStream.Seek(0, SeekOrigin.Begin);
        var thumbnailImage = new MagickImage(imageStream);
        thumbnailImage.Resize(width, height);
        var result = new MemoryStream();

        await thumbnailImage.WriteAsync(result);
        return result;
    }

    public async Task StoreImageWithThumbnail(Stream imageStream, string path, string thumbnailPath)
    {
        //Make thumbnail
        using var thumbnailStream = await MakeImageThumbnail(imageStream, ImageConstants.ThumbnailSize, ImageConstants.ThumbnailSize);

        //Save files in storage
        imageStream.Seek(0, SeekOrigin.Begin);
        thumbnailStream.Seek(0, SeekOrigin.Begin);
        await _storage.PutFiles([
            new (path, imageStream),
            new (thumbnailPath, thumbnailStream)
        ]);
    }
}
