using Microsoft.Extensions.Options;

namespace api.Helpers.Storage;

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
        EnsureDirectoryExists(_storageOptions.Value.LocalFolderPath);

        var newFile = File.Open(FullPath(path), FileMode.Create);
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
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
