using Microsoft.Extensions.Options;

namespace BusinessLogic.Helpers.Storage;

public class LocalFileStorage : IStorage
{
    private readonly IOptions<StorageOptions> _storageOptions;
    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        _storageOptions = options;
    }

    public Task DeleteFile(string path)
    {
        File.Delete(Path.Combine(FullPath(path)));
        return Task.CompletedTask;
    }

    public Task<Stream> GetFile(string path)
    {
        return Task.FromResult<Stream>(File.OpenRead(Path.Combine(FullPath(path))));
    }

    public async Task PutFile(string path, Stream fileStream)
    {
        var fullPath = FullPath(path);
        EnsureDirectoryExists(fullPath);

        var newFile = File.Open(fullPath, FileMode.Create);
        try
        {
            await fileStream.CopyToAsync(newFile);
        } finally
        {
            newFile.Dispose();
            fileStream.Dispose();
        }
    }

    protected string FullPath(string filePath)
    {
        EnsureDirectoryExists(_storageOptions.Value.LocalFolderPath);
        var p = Path.Combine(_storageOptions.Value.LocalFolderPath, filePath);
        return p;
    }

    public void EnsureDirectoryExists(string path)
    {
        var directoryPath = Path.GetDirectoryName(path);
        if (directoryPath is null)
        {
            //No directory in provided path, return
            return;
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
