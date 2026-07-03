using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoDbContext _db;
    public TodosController(TodoDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public ActionResult<List<TodoItem>> GetAll()
    {
        return _db.Todos.OrderBy(t => t.Id).ToList();
    }
    [HttpGet("{id}")]
    public ActionResult<TodoItem> Get(int id)
    {
        var todo = _db.Todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return NotFound();
        return todo;
    }
    [HttpPost]
    public ActionResult<TodoItem> Create(TodoItem newtodo)
    {
        if (string.IsNullOrWhiteSpace(newtodo.Title))
            return BadRequest("Title is required.");
        var todo = new TodoItem { Title = newtodo.Title };
        _db.Todos.Add(todo);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }
    [HttpPut("{id}/complete")]
    public IActionResult MarkComplete(int id)
    {
        var todo = _db.Todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return NotFound();
        todo.IsDone = true;
        _db.SaveChanges();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var todo = _db.Todos.FirstOrDefault(t => t.Id == id);
        if (todo == null) return NotFound();
        _db.Todos.Remove(todo);
        _db.SaveChanges();
        return NoContent();
    }
}    