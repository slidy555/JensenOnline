using JENSENOnline.API.Data;
using JENSENOnline.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JENSENOnline.API.Controllers;

public class LoginData
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterData : LoginData
{
    public string Name { get; set; } = "";
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public const string JwtKey =
        "JENSENOnline-Local-Demo-Key-2026-Only!";

    private readonly AppDbContext db;
    private readonly PasswordHasher<User> hasher = new();

    public AuthController(AppDbContext context)
    {
        db = context;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name) ||
            string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrWhiteSpace(data.Password) ||
            data.Password.Length < 8)
        {
            return BadRequest("Fyll i alla fält. Lösenordet måste ha minst 8 tecken.");
        }

        string email = data.Email.Trim().ToLower();

        if (db.Users.Any(u => u.Email == email))
            return BadRequest("E-posten används redan.");

        var user = new User
        {
            Name = data.Name.Trim(),
            Email = email,
            Role = "Customer"
        };

        user.PasswordHash = hasher.HashPassword(user, data.Password);

        db.Users.Add(user);
        db.SaveChanges();

        return Ok("Kontot skapades.");
    }

    [HttpPost("login")]
    public IActionResult Login(LoginData data)
    {
        string email = data.Email.Trim().ToLower();

        var user = db.Users.FirstOrDefault(u => u.Email == email);

        if (user == null ||
            hasher.VerifyHashedPassword(
                user, user.PasswordHash, data.Password)
                == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Fel e-post eller lösenord.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(JwtKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            role = user.Role
        });
    }
}