namespace BusinessLogic.Helpers.FilePathResolver;

public interface IFilePathResolver
{
    public string GenerateUniqueFilePath(string prefix, string fileName);
}
