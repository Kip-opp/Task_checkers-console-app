using Microsoft.EntityFrameworkCore;

namespace Shared;

public class TodoDbContext : DbContext
{
    public TodoDbContext()
    {
    }

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> Todos { get; set; } 
    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured) options.UseSqlite("Data Source=todos.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.NormalizedUsername).IsUnique();
        modelBuilder.Entity<TodoItem>().Property(todo => todo.Title).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<TodoItem>().HasOne(todo => todo.Owner).WithMany(user => user.Todos).HasForeignKey(todo => todo.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}    