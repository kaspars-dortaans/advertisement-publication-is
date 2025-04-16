using BusinessLogic.Entities;
using BusinessLogic.Entities.Files;

namespace BusinessLogic.Services;

public class FileService(Context dbContext,
    IBaseService<MessageAttachment> messageAttachmentService
) : BaseService<Entities.Files.File>(dbContext), IFileService
{
    private readonly IBaseService<MessageAttachment> _messageAttachmentService = messageAttachmentService;

    public async Task<bool> HasAccessToFile(Entities.Files.File file, int? userId)
    {
        if (file.IsPublic)
        {
            return true;
        }

        switch (file)
        {
            case UserImage:
                var isFileOwner = file is UserImage && userId == (file as UserImage)!.OwnerUserId;

                if (isFileOwner)
                {
                    return true;
                }
                break;
            case MessageAttachment:
                var isChatMember = await _messageAttachmentService
                    .ExistsAsync(attachment => attachment.Id == file.Id 
                        && attachment.Message.Chat.ChatUsers.Any(u => u.UserId == userId));

                if (isChatMember)
                {
                    return true;
                }
                break;
        }
        return false;
    }
}
