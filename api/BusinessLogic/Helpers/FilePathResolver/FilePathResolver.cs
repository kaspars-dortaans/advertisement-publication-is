namespace BusinessLogic.Helpers.FilePathResolver;

public class FilePathResolver : IFilePathResolver
{
    private const char FileNameDelimiter = '-';

    public string GenerateUniqueFilePath(string folder, string fileName)
    {
        var timeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        return Path.Combine(folder, $"{timeStamp}{FileNameDelimiter}{fileName}");
    }

    public string GetOriginalFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }

        var fileName = Path.GetFileName(path);
        return fileName[(fileName.IndexOf(FileNameDelimiter) + 1)..];
    }
}
