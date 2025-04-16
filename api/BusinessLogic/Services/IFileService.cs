namespace BusinessLogic.Services;

public interface IFileService : IBaseService<Entities.Files.File>
{
    public Task<bool> HasAccessToFile(Entities.Files.File file, int? userId);
}
