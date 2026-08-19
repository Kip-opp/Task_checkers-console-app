namespace Shared;

public class TodoItem
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        string status = IsDone ? "[X]" : "[ ]";
        return $"{status} {Title} (Created at: {CreatedAt:MM/dd/yyyy HH:mm:ss})";
    }
}