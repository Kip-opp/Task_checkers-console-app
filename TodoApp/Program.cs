using TodoApp;

List<TodoItem> todos = new List<TodoItem>();
int nextId = 1;
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
    if (todos.Count == 0)
    {
        Console.WriteLine("No tasks available.");
        return;
    }

    Console.WriteLine("\n-- TASK LIST --");
    foreach (var todo in todos)
    {
        string status = todo.IsDone ? "[X]" : "[ ]";
        Console.WriteLine($"{todo.Id}. {status} {todo.Title}");
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

    todos.Add(new TodoItem { Id = nextId, Title = title });
    Console.WriteLine($"Added task: {title}");
    nextId++;
}

void CompleteTask()
{
    Console.Write("Enter task ID to mark as complete: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var todo = todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
        {
            todo.IsDone = true;
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
        int removed = todos.RemoveAll(t => t.Id == id);
        Console.WriteLine(removed > 0 ? $"Task with ID {id} deleted." : "Task not found.");
    }
    else
    {
        Console.WriteLine("Invalid ID.");
    }
}


