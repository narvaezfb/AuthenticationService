using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models
{
	public class UserUpdateModel
	{
        [Required(ErrorMessage = "First Name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = " Age is required")]
        [Range(18, 99, ErrorMessage = "Age must be between 18 and 99.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string Email { get; set; }

    }
}

