using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models.RequestModels
{
	public class SignupModel
	{
        //public int UserId { get; set; }
		[Required(ErrorMessage ="Username is required!")]
		public required string Username { get; set; }

        [Required(ErrorMessage = "Username is required!")]
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Age is required!")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Location is required!")]
        public required string Location { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Password Confirm is required!")]
        public required string PasswordConfirm { get; set; }

        public bool ConfirmPasswords(string inputPassword, string inputConfirmPassword)
        {
            return inputPassword == inputConfirmPassword;
        }

    }
}

