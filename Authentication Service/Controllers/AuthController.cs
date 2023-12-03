using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication_Service.Models;
using Authentication_Service.Responses;
using Authentication_Service.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Authentication_Service.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class AuthController : ControllerBase
    {
        private static string? _jwtKey;
        private static string? _jwtIssuer;
        private static string? _jwtAudience;

        private readonly AuthenticationServiceDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthenticationServiceDbContext context, IConfiguration configuration)
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

        [HttpPost("validateToken", Name = "ValidateToken")]
        [AllowAnonymous]
        public ActionResult ValidateToken([FromBody] string token)
        {
            TokenValidationResponse response = new TokenValidationResponse();
          
            if (string.IsNullOrWhiteSpace(token))
            {

                response = new TokenValidationResponse("FAILURE", 400, false, "Empty token");
                return BadRequest(response);
            }

            try
            {
                // Configure token validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtIssuer,
                    ValidAudience = _jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey))
                };

                // Token validation using JwtSecurityTokenHandler
                var tokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // Extract claims from the validated token
                var validatedJwt = validatedToken as JwtSecurityToken;
                if (validatedJwt == null)
                {
                    response = new TokenValidationResponse("FAILURE", 401, false, "Invalid Token");
                    return BadRequest(response);
                }

                response = new TokenValidationResponse("SUCCESS", 200, true, null);
                return Ok(response);
            }
            catch (SecurityTokenException)
            {
                response = new TokenValidationResponse("FAILURE", 401, false, "Invalid token");
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while validating the token: {ex.Message}");
            }
        }

        [HttpPost("validateAdminToken", Name = "ValidateAdminToken")]
        [AllowAnonymous]
        public ActionResult ValidateAdminToken([FromBody] string token)
        {
            TokenValidationResponse response = new TokenValidationResponse();

            if (string.IsNullOrWhiteSpace(token))
            {

                response = new TokenValidationResponse("FAILURE", 400, false, "Empty token");
                return BadRequest(response);
            }

            try
            {
                // Configure token validation parameters
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtIssuer,
                    ValidAudience = _jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey))
                };

                // Token validation using JwtSecurityTokenHandler
                var tokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // Extract claims from the validated token
                var validatedJwt = validatedToken as JwtSecurityToken;
                if (validatedJwt == null)
                {
                    response = new TokenValidationResponse("FAILURE", 401, false, "Invalid Token");
                    return BadRequest(response);
                }

               
                if (!claimsPrincipal.IsInRole("Admin"))
                {
                    response = new TokenValidationResponse("FAILURE", 401, false, "Role does not have permissions");
                    return BadRequest(response);
                }

                response = new TokenValidationResponse("SUCCESS", 200, true, null);
                return Ok(response);
            }
            catch (SecurityTokenException)
            {
                response = new TokenValidationResponse("FAILURE", 401, false, "Invalid token");
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while validating the token: {ex.Message}");
            }
        }

        [HttpPost("Signup", Name = "Signup")]
        public async Task<ActionResult> CreateUser([FromBody] SignupModel signupModel)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("User data is invalid.");
            }

            if (!signupModel.ConfirmPasswords(signupModel.Password, signupModel.PasswordConfirm))
            {
                return BadRequest("Passwords do not match");
            }
            const int userRoleID = 3;

            User user = new User(signupModel.Username, signupModel.Email, signupModel.Age, signupModel.Location, signupModel.Password, userRoleID);
            user.HashPassword(user.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var token = await GenerateJwtTokenAsync(user.Email, new[] {"User"}, _jwtKey, _jwtIssuer, _jwtAudience);

            var responseData = new
            {
                user,
                token
            };

            return Ok(responseData);
        }

        [HttpPost("Login", Name = "Login")]
        public async Task<ActionResult> Login([FromBody] Login login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email).ConfigureAwait(false);

            if (user == null || user.VerifyPassword(login.Password, user.Password) == false)
            {
                return BadRequest("Authentication failed");
            }

            var roles = await createListOfRoles(user.RoleID);

            var token = await GenerateJwtTokenAsync(user.Email, roles, _jwtKey, _jwtIssuer, _jwtAudience);

            return Ok(new { token, user });

        }

        [HttpPost("ForgotPassword", Name = "ForgotPassword")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordModel.Email);

            if (user == null)
            {
                return BadRequest("User not found with the provided email address");
            }

            // Generate a password reset token and save it to the user's record
            var token = user.GeneratePasswordResetToken();

            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Set token expiration time

            // Save the changes to the database
            await _context.SaveChangesAsync();

            try
            {
                // Send an email to the user with instructions to reset the password
                await user.SendPasswordResetEmail(user.Email, token);

            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending the email to use error: {ex}");
            }


            return Ok("Password reset instructions sent to the provided email address");
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

        private async Task<string[]> createListOfRoles(int RoleID)
        {
            var role = await _context.Roles.FindAsync(RoleID);
            string[] roles = new string[] { role.Name };
            return roles;
            
        }

    }
}

