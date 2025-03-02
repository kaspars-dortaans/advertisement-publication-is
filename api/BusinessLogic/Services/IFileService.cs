namespace BusinessLogic.Services;

public interface IFileService : IBaseService<Entities.Files.File>
{
    public bool HasAccessToFile(Entities.Files.File file, int? userId);
}
