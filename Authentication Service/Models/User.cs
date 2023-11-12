using System;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace Authentication_Service.Models
{
	public class User
	{
        public int UserId { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required int Age { get; set; }

        public required string Email { get; set; }

		public required string Password { get; set; }

        public required string PasswordConfirm { get; set; }

        
        public bool ConfirmPasswords(string inputPassword, string inputConfirmPassword)
        {
            return inputPassword == inputConfirmPassword;
        }

       
        public string? HashPassword(string inputPassword)
        {
            if (!string.IsNullOrEmpty(inputPassword))
            {
                string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(inputPassword);
                Password = encryptedPassword;
                PasswordConfirm = "";
                return encryptedPassword;
            }
            return null;

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



        public bool IsValidEmail(string email)
        {
            // Regular expression for a basic email validation
            string pattern = @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$";

            // Check if the provided email matches the pattern
            return Regex.IsMatch(email, pattern);
        }

    }
}

