using System;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using Authentication_Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication_Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
	{
        private readonly AuthenticationServiceDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AuthenticationServiceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Signup", Name = "Signup")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Person data is invalid.");
            }

            if (!user.IsValidEmail(user.Email))
            {
                return BadRequest("Email is not valid");
            }

            if (!user.ConfirmPasswords(user.Password, user.PasswordConfirm))
            {
                return BadRequest("Passwords do not match");
            }

            user.HashPassword(user.Password);

            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("Login", Name = "Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (string.IsNullOrEmpty(login.Email))
            {
                return BadRequest("Email cannot be null");
            }

            if (string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Password cannot be null");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == login.Email).ConfigureAwait(false);

            if (user == null || user.VerifyPassword(login.Password, user.Password) == false)
            {
                return BadRequest("Authentication failed");
            }

            // Get JWT configuration values from appsettings.json
            var key = _configuration["JwtSettings:Key"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            // Generate a JWT token without authentication
            var token = GenerateJwtToken(user.Email, new[] { "admin" }, key, issuer, audience);

            return Ok(new { token });

        }

        private string GenerateJwtToken(string username, string[] roles, string key, string issuer, string audience)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

    }
}


