using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models
{
	public class UserUpdateModel
	{
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = " Age is required")]
        [Range(18, 99, ErrorMessage = "Age must be between 18 and 99.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public required string Location { get; set; }

    }
}

