using Microsoft.AspNetCore.Mvc;
using Authentication_Service.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace Authentication_Service.Controllers
{
    
    [ApiController]
    [Route("[Controller]")]
    public class UserController : ControllerBase
	{
        private readonly AuthenticationServiceDbContext _context;

        public UserController(AuthenticationServiceDbContext context)
        {
            _context = context;
        }

       
        [Authorize]
        [HttpPatch("UpdatePassword/{userId}", Name= "UpdatePassword")]
        public async Task<IActionResult> UpdateUserPassword(int userId, [FromBody] UserPasswordUpdateModel userPasswordUpdateModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.User.FindAsync(userId);

                if (user == null)
                {
                    return BadRequest("user not found with that ID");
                }

                if (!user.VerifyPassword(userPasswordUpdateModel.CurrentPassword, user.Password))
                {
                    return BadRequest("Invalid current password");
                }

                if (!user.ConfirmPasswords(userPasswordUpdateModel.NewPassword, userPasswordUpdateModel.NewPasswordConfirm))
                {
                    return BadRequest("new passwords do not match");
                }


                user.Password = user.HashPassword(userPasswordUpdateModel.NewPassword);


                await _context.SaveChangesAsync();

                return Ok("success pasword update");
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the password");
            }
            
        }

        [Authorize]
        [HttpPatch("EditUserAccount/{userId}", Name = "EditUserAccount")]
        public async Task<ActionResult> EditUserInformation(int userId, [FromBody] JsonPatchDocument<UserUpdateModel> patchDoc)
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

        [Authorize]
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
    }
}


