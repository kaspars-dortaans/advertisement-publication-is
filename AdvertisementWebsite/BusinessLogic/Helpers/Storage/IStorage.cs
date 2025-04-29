namespace BusinessLogic.Helpers.Storage;

public interface IStorage
{
    public Task PutFile(string path, Stream stream);
    public Task PutFiles(IEnumerable<KeyValuePair<string, Stream>> files);
    public Task<Stream> GetFile(string path);
    public Task DeleteFile(string path);
    public Task DeleteFiles(IEnumerable<string> paths);
}
