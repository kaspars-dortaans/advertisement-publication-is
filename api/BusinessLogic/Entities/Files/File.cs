namespace BusinessLogic.Entities.Files;

public class File
{
    public int Id { get; set; }
    public string Path { get; set; } = default!;
    public bool IsPublic { get; set; } = false;
}