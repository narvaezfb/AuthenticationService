using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication_Service.Models
{
    public class UserPasswordUpdateModel
    {
        [Required(ErrorMessage = "Current Password is required")]
        public required string CurrentPassword { get; set; }
        [Required(ErrorMessage = "New Password  is required")]
        public required string NewPassword {get;set;}
        [Required(ErrorMessage = "New Password Confirm is required")]
        public required string NewPasswordConfirm { get; set; }

        public bool ConfirmPasswords(string inputPassword, string inputConfirmPassword)
        {
            return inputPassword == inputConfirmPassword;
        }

    }
}

