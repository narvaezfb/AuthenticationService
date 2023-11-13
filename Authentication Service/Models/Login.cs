using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models
{
	public class Login
	{
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
        
	}
}

