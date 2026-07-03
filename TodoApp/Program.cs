using Microsoft.EntityFrameworkCore;
using TodoApp;

using var db = new TodoDbContext();
db.Database.EnsureCreated();

bool running = true;

while (running)
{
    Console.WriteLine("\n-- TODO APP --");
    Console.WriteLine("1. Add a new task");
    Console.WriteLine("2. List all tasks");
    Console.WriteLine("3. Complete task");
    Console.WriteLine("4. Delete task");
    Console.WriteLine("5. Exit");
    Console.Write("Choose an option: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            AddTask();
            break;
        case "2":
            ListTasks();
            break;
        case "3":
            CompleteTask();
            break;
        case "4":
            DeleteTask();
            break;
        case "5":
            running = false;
            Console.WriteLine("Goodbye!");
            break;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;                    
    }
}

void ListTasks()
{
    var todos = db.Todos.OrderBy(t => t.Id).ToList();
    if (todos.Count == 0)
    {
        Console.WriteLine("No tasks available.");
        return;
    }

    foreach (var todo in todos)
    {
        
        Console.WriteLine(todo);
    }
}

void AddTask()
{
    Console.Write("Enter task title: ");
    string? title = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Task title cannot be empty.");
        return;
    }

    db.Todos.Add(new TodoItem { Title = title });
    db.SaveChanges();
    Console.WriteLine($"Task '{title}' added successfully.");
}

void CompleteTask()
{
    Console.Write("Enter task ID to mark as complete: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var todo = db.Todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.IsDone = true;
            db.SaveChanges();
            Console.WriteLine($"Task '{todo.Title}' marked as complete.");
        }
        else
        {
            Console.WriteLine("Task not found.");
        }
    }
    else
    {
        Console.WriteLine("Invalid ID.");
    }
}

void DeleteTask()
{
    Console.Write("Enter task ID to delete: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var todo = db.Todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            db.Todos.Remove(todo);
            db.SaveChanges();
            Console.WriteLine($"Task with ID {id} deleted.");
        }
        else
        {
            Console.WriteLine("Task not found.");
        }
    }
    else
    {
        Console.WriteLine("Invalid ID.");
    }
}


