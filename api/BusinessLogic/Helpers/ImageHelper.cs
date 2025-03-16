using ImageMagick;

namespace BusinessLogic.Helpers;

public class ImageHelper
{
    public static async Task<Stream> MakeImageThumbnail(Stream imageStream, uint width, uint height)
    {
        var thumbnailImage = new MagickImage(imageStream);
        thumbnailImage.Resize(width, height);
        var result = new MemoryStream();

        await thumbnailImage.WriteAsync(result);
        return result;
    }
}
