namespace AdvertisementWebsite.Server.Dto.User;

public class PublicUserInfoDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LinkToUserSite {  get; set; }
    public string? ProfileImageUrl { get; set; }

}
