using Microsoft.EntityFrameworkCore;

namespace Shared;

public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> Todos { get; set; } 
    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=todos.db");
    }
}    