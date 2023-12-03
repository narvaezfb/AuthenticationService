using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Authentication_Service.Models
{
	public class Role
	{
        [Required(ErrorMessage = "Role ID is required")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role Description is required")]
        public string Description { get; set; }

        [JsonIgnore]
        public ICollection<User> Users { get; set; }
    }
}

