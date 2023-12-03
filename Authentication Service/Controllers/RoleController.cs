using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Authentication_Service.Models.RequestModels.RoleRequest;
using Authentication_Service.Models;
using Microsoft.AspNetCore.Authorization;

namespace Authentication_Service.Controllers
{
    [Route("[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly AuthenticationServiceDbContext _context;
        private readonly IConfiguration _configuration;

        public RoleController(AuthenticationServiceDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("RetrieveAllRoles", Name = "Retrieve All Roles")]
        public ActionResult GetRoles()
        {
            var roles = _context.Roles.ToList();
            return Ok(roles);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("RetrieveOneRole/{id}", Name ="Get single Role")]
        public async Task<ActionResult> GetOneRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if(role == null)
            {
                return BadRequest("Not role found with that ID");
            }

            return Ok(role);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateRole", Name = "Create Role")]
        public async Task<ActionResult> CreateRole([FromBody] CreateRole createRole)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Create request body is incorrect");
            }
            Role role = new()
            {
                Name = createRole.Name,
                Description = createRole.Description
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return Ok(role);
        }
    }
}

