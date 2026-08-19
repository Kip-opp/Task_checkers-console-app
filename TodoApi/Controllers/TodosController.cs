using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed record TodoCreateRequest(string Title);
public sealed record TodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt);

public class TodosController : ControllerBase
{
    private readonly TodoDbContext _db;
    public TodosController(TodoDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();
        var todos = await _db.Todos.AsNoTracking().Where(todo => todo.OwnerId == ownerId).OrderBy(todo => todo.Id).Select(todo => new TodoResponse(todo.Id, todo.Title, todo.IsDone, todo.CreatedAt)).ToListAsync(cancellationToken);
        return Ok(todos);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoResponse>> Get(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();
        var todo = await _db.Todos.AsNoTracking().Where(item => item.Id == id && item.OwnerId == ownerId).Select(item => new TodoResponse(item.Id, item.Title, item.IsDone, item.CreatedAt)).SingleOrDefaultAsync(cancellationToken);
        return todo is null ? NotFound() : Ok(todo);
    }
    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create(TodoCreateRequest request, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();
        var title = request.Title?.Trim();
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200) return BadRequest(new { error = "Title is required and must be 200 characters or fewer." });
        var todo = new TodoItem { OwnerId = ownerId.Value, Title = title };
        _db.Todos.Add(todo);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, new TodoResponse(todo.Id, todo.Title, todo.IsDone, todo.CreatedAt));
    }
    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> MarkComplete(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();
        var todo = await _db.Todos.SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (todo is null) return NotFound();
        todo.IsDone = true;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = GetOwnerId();
        if (ownerId is null) return Unauthorized();
        var todo = await _db.Todos.SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);
        if (todo is null) return NotFound();
        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private int? GetOwnerId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;
}    