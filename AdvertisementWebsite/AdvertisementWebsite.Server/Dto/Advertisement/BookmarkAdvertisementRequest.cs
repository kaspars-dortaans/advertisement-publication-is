using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace AdvertisementWebsite.Server.Dto.Advertisement;

public class BookmarkAdvertisementRequest
{
    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public int AdvertisementId { get; set; }

    [Required(ErrorMessage = CustomErrorCodes.MissingRequired)]
    public bool AddBookmark { get; set; }
}
