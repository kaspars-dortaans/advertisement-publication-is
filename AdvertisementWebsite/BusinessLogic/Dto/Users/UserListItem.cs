namespace BusinessLogic.Dto.Users;

public class UserListItem
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? UserName { get; set; } = default!;
    public IEnumerable<string?> UserRoles { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string? PhoneNumber { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastActive { get; set; }
}
