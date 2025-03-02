using BusinessLogic.Entities;
using BusinessLogic.Entities.Files;

namespace BusinessLogic.Services;

public class FileService(Context dbContext) : BaseService<Entities.Files.File>(dbContext), IFileService
{
    public bool HasAccessToFile(Entities.Files.File file, int? userId)
    {
        if (file.IsPublic)
        {
            return true;
        }

        switch (file)
        {
            case UserImage:
                var isFileOwner = file is UserImage && userId == (file as UserImage)!.OwnerUserId;

                if (!isFileOwner)
                {
                    return false;
                }
                break;
        }
        return false;
    }
}
