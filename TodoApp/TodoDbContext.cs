using Microsoft.EntityFrameworkCore;

namespace TodoApp;

public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> Todos { get; set; } 
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=todos.db");
    }
}    