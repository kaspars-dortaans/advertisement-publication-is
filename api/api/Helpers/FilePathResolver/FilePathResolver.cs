namespace api.Helpers.FilePathResolver;

public class FilePathResolver : IFilePathResolver
{
    public string GenerateUniqueFilePath(string folder, string fileName)
    {
        var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        return Path.Combine(folder, $"{timeStamp}-{fileName}");
    }
}
