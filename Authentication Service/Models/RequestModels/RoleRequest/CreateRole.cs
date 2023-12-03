using System.ComponentModel.DataAnnotations;
namespace Authentication_Service.Models.RequestModels.RoleRequest
{
	public class CreateRole
	{
		[Required(ErrorMessage ="Role name is Required")]
		public string Name { get; set; }

        [Required(ErrorMessage = "Role description is Required")]
        public string Description { get; set; }
	}
}

