using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models
{
	public class User
	{
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Age is required")]
        [Range(18, 99, ErrorMessage = "Age must be between 18 and 99.")]
        public required int Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Password Confirm is required")]
        public required string PasswordConfirm { get; set; }

        
        public bool ConfirmPasswords(string inputPassword, string inputConfirmPassword)
        {
            return inputPassword == inputConfirmPassword;
        }


        public string HashPassword(string inputPassword)
        {
            if (string.IsNullOrEmpty(inputPassword))
            {
                throw new ArgumentException("Input password cannot be null or empty.");
            }

            string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(inputPassword);
            Password = encryptedPassword; 
            PasswordConfirm = ""; 

            return encryptedPassword;
        }


        public bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            if (string.IsNullOrEmpty(enteredPassword) || string.IsNullOrEmpty(hashedPassword))
            {
                // Handle invalid input (e.g., log, throw an exception, etc.)
                return false;
            }
            // Compare the entered password with the stored hashed password
            return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
        }

    }
}

