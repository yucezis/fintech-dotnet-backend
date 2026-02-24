using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenBudget.Application.DTOs;
using ZenBudget.Domain.Entities;
using ZenBudget.Infrastructure.Persistence;

namespace ZenBudget.API.Controllers;

[ApiController]
[Route("api/v1/auth")] 
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Bu e-posta adresi zaten kullanýlýyor.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanýcý baþarýyla oluþturuldu." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("E-posta veya þifre hatalý.");

        return Ok(new { message = "Giriþ baþarýlý!", userId = user.Id, fullName = user.FullName });
    }
}