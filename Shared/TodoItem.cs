namespace Shared;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public override string ToString()
    {
        string status = IsDone ? "[X]" : "[ ]";
        return $"{status} {Title} (Created at: {CreatedAt:MM/dd/yyyy HH:mm:ss})";
    }
}