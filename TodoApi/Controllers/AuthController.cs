using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace TodoApi.Controllers;

public record AuthRequest(string Username, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TodoDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(TodoDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(AuthRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim();
        if (string.IsNullOrWhiteSpace(username) || username.Length is < 3 or > 64 || request.Password is null || request.Password.Length < 10)
            return BadRequest(new { error = "Username must be 3-64 characters and password must be at least 10 characters." });
        var normalized = username.ToUpperInvariant();
        if (await _db.Users.AnyAsync(user => user.NormalizedUsername == normalized, cancellationToken)) return Conflict("Unable to register with those credentials.");
        _db.Users.Add(new User { Username = username, NormalizedUsername = normalized, PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12) });
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Account registered." });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(AuthRequest request, CancellationToken cancellationToken)
    {
        var normalized = request.Username?.Trim().ToUpperInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(item => item.NormalizedUsername == normalized, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });

    }

}