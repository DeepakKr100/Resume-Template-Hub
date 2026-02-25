using EaseMyBooking.Api.Data;
using EaseMyBooking.Api.Dtos;
using EaseMyBooking.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EaseMyBooking.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    public AuthController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already registered.");

        if (!Enum.TryParse<Role>(request.Role, true, out var parsedRole))
            return BadRequest("Invalid role. Allowed: Visitor, Owner");

        if (parsedRole == Role.Admin)
            return BadRequest("Admin signup is not allowed.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Role = parsedRole
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok("Registration successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto credentials)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == credentials.Email);
        if (user == null || !VerifyPassword(credentials.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password");

        string token = GenerateJwtToken(user);
        return Ok(new { token, userId = user.UserId, name = user.Name, role = user.Role.ToString() });
    }

    // --- helpers ---

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string storedHash)
        => HashPassword(password) == storedHash;

    private SymmetricSecurityKey ResolveSigningKey()
    {
        var secret = _config["JwtSettings:SecretKey"]
                     ?? throw new Exception("Missing JwtSettings:SecretKey");

        // IMPORTANT: mirror Program.cs behavior (support base64: prefix)
        byte[] keyBytes = secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase)
            ? Convert.FromBase64String(secret["base64:".Length..])
            : Encoding.UTF8.GetBytes(secret);

        return new SymmetricSecurityKey(keyBytes);
    }

    private string GenerateJwtToken(User user)
    {
        var key = ResolveSigningKey();
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "ResumeTemplateHubAPI",   // MUST match Program.cs
            audience: "ResumeTemplateHubAPI", // MUST match Program.cs
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
