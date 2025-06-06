﻿using BusinessLogic.Entities.Files;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsPhoneNumberPublic { get; set; }
    public bool IsEmailPublic { get; set; }
    public string? LinkToUserSite { get; set; }
    public int? ProfileImageFileId { get; set; }
    public UserImage ProfileImageFile { get; set; } = default!;
    public ICollection<Advertisement> OwnedAdvertisements { get; set; } = default!;
    public ICollection<Advertisement> BookmarkedAdvertisements { get; set; } = default!;
    public ICollection<AdvertisementBookmark> AdvertisementBookmarks { get; set; } = default!;
    public ICollection<Chat> Chats { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastActiveDate { get; set; }

}
