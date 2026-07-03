using Microsoft.AspNetCore.Mvc;
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
    public IActionResult Register(AuthRequest request)
    {
        if (_db.Users.Any(u => u.Username == request.Username))
        return BadRequest("Username already exists.");
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
         _db.Users.Add(user);
         _db.SaveChanges();

         return Ok("User registered successfully.");
            
}

    [HttpPost("Login")]
    public IActionResult Login(AuthRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var token = _tokenService.GenerateToken(user.Username);
        return Ok(new { token });

    }

}