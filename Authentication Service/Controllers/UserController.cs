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
using Microsoft.AspNetCore.JsonPatch;
using System.Xml.XPath;
using Newtonsoft.Json.Linq;

namespace Authentication_Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
	{
        private static string? _jwtKey;
        private static string? _jwtIssuer;
        private static string? _jwtAudience;

        private readonly AuthenticationServiceDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AuthenticationServiceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            if (string.IsNullOrEmpty(_jwtKey))
            {
                _jwtKey = _configuration["JwtSettings:Key"];
                _jwtIssuer = _configuration["JwtSettings:Issuer"];
                _jwtAudience = _configuration["JwtSettings:Audience"];
            }
        }

        [HttpPost("Signup", Name = "Signup")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Person data is invalid.");
            }

            if (!user.ConfirmPasswords(user.Password, user.PasswordConfirm))
            {
                return BadRequest("Passwords do not match");
            }

            user.HashPassword(user.Password);

            _context.User.Add(user);

            await _context.SaveChangesAsync();

            // Generate a JWT token without authentication
            var token = await GenerateJwtTokenAsync(user.Email, new[] { "admin" }, _jwtKey, _jwtIssuer, _jwtAudience);

            var responseData = new 
            {
               user,
               token
            };

            return Ok(responseData);
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

            // Generate a JWT token without authentication
            var token = await GenerateJwtTokenAsync(user.Email, new[] { "admin" }, _jwtKey, _jwtIssuer, _jwtAudience);

            return Ok(new { token });

        }

        [HttpPatch("EditUserAccount/{userId}", Name = "EditUserAccount")]
        public async Task<ActionResult> JsonPatchWithModelState(int userId, [FromBody] JsonPatchDocument<UserUpdateModel> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Not model included");
            }

            var user = await _context.User.FindAsync(userId);

            if(user == null)
            {
                return BadRequest("No user found with that email");
            }

            // Create a UserUpdateModel instance and apply the patch document
            var userToPatch = new UserUpdateModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Age = user.Age,
                Email = user.Email
            };

            // Apply the patch document to the UserUpdateModel
            patchDoc.ApplyTo(userToPatch, ModelState);

            // Validate the model state after applying the patch
            if (!TryValidateModel(userToPatch))
            {
                return BadRequest(ModelState);
            }

            // Update attributes in the user entity
            user.FirstName = userToPatch.FirstName;
            user.LastName = userToPatch.LastName;
            user.Age = userToPatch.Age;
            user.Email = userToPatch.Email;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpDelete("DeleteAccount/{userId}", Name = "DeleteAccount")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Not user found with that ID");
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private async Task<string> GenerateJwtTokenAsync(string username, string[] roles, string key, string issuer, string audience)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            // Add roles to claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return await Task.FromResult(tokenString);
        }

    }
}


