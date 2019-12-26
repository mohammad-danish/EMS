using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EMS.Api.Factory;
using EMS.Shared.Constants;
using EMS.Shared.DTOs.Account;
using EMS.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        #region Private Properties

        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        private readonly TokenConfigOption tokenOptions;

        #endregion


        #region Constructor
        public AccountController
            (UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager, IOptions<TokenConfigOption> tokenOptions)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenOptions = tokenOptions.Value;
            this.roleManager = roleManager;
        }

        #endregion


        #region Login/Register


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            var user = await userManager.FindByEmailAsync(login.EmailId);

            if(user is null)
            { return BadRequest("User NOT EXISTS"); }

            var response = await signInManager.PasswordSignInAsync(login.EmailId, login.Password, false, true);
            
            if (!response.Succeeded)
            {
                return Unauthorized("Either username or password is incorrect");
            }
            var roles = await userManager.GetRolesAsync(user);
            var claims = await userManager.GetClaimsAsync(user);
            return Ok(new { token = TokenFactory.Create(tokenOptions, user.UserName, roles, claims) });

        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model is not valid");
            }

            var user = new IdentityUser { UserName = registerDTO.EmailId, Email = registerDTO.EmailId };

            var response = await userManager.CreateAsync(user, registerDTO.Password);

            return Ok(response);

        }

        #endregion


        #region Manage Application Roles

        [HttpGet("roles")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetRoles()
            => await Task.FromResult(Ok(roleManager.Roles));

        [HttpPost("roles")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AddRoles([FromBody] string[] newRoles)
        {
            if (newRoles is null || newRoles.Length <= 0)
            {
                return BadRequest("newRoles cannot be null or empty.");
            }

            var list = new List<IdentityResult>();
            foreach (var roleName in newRoles)
            {
                var role = new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                };

                list.Add(await roleManager.CreateAsync(role));
            }

            return Ok(list);
        }


        [HttpDelete("roles")]
        [Authorize(Roles = Role.SuperAdmin)]
        public async Task<IActionResult> DeleteRoles([FromBody] string[] deleteRoleList)
        {
            if (deleteRoleList is null || deleteRoleList.Length <= 0)
            {
                return BadRequest("deleteRoleList cannot be null or empty.");
            }

            var list = new List<IdentityResult>();
            foreach (var roleName in deleteRoleList)
            {
                var role = await roleManager.FindByNameAsync(roleName);

                if (role is null) { continue; }

                list.Add(await roleManager.DeleteAsync(role));
            }

            return Ok(list);
        }

        #endregion


        #region Manage User Roles
        [HttpGet("user-roles")]
        public async Task<IActionResult> GetUserRoles([FromQuery] string emailId)
        {
            if(string.IsNullOrEmpty(emailId)) 
                { return BadRequest("EmailId cannot be null or empty.");  }

            var user = await userManager.FindByEmailAsync(emailId);

            if(user is null) 
                { return BadRequest($"No user with '{emailId}' email exits."); }

            var roles = await userManager.GetRolesAsync(user);

            var response = new { email = emailId, userRoles = roles };

            return Ok(response);
        }

        [HttpPost("user-roles")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AddUserRoles([FromBody] ManageUserRoleDTO model)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid model"); }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null)
            { return BadRequest($"No user with '{model.Email}' email exits."); }

            var response = await AddRolesToUser(user, model);

            return Ok(response);
        }

        [HttpDelete("user-roles")]
        [Authorize(Policy = "canDeleteUserRoles")]
        public async Task<IActionResult> DeleteUserRoles([FromBody] ManageUserRoleDTO model)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid Request model."); }

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null)
            { return BadRequest($"No user with '{model.Email}' email exits."); }

            var response = await userManager.RemoveFromRolesAsync(user, model.Roles);
            return Ok(response);

        }


        private async  Task<List<StatusDTO<int, bool>>> AddRolesToUser(IdentityUser user, ManageUserRoleDTO model)
        {
            var response = new List<StatusDTO<int, bool>>();

            foreach (var role in model.Roles)
            {
                var status = new StatusDTO<int, bool> { Name = role};

                if (!await roleManager.RoleExistsAsync(role))
                {
                    status.Status = false;
                    status.Error = $"{role} NOT_FOUND";
                    response.Add(status);
                    continue;
                }

                var result = await userManager.AddToRoleAsync(user, role);
                status.Status = result.Succeeded;
                status.Error = string.Join('\r', result.Errors.Select(e => e.Description));
                response.Add(status);
            }

            return response;
        }

        #endregion


        #region Manage User's Claims

        [HttpGet("user-claims")]
        public async Task<IActionResult> GetUserClaims([FromQuery] string emailId)
        {

            var user = await userManager.FindByEmailAsync(emailId);
            if(user is null)
            { return BadRequest($"No user exits with '{emailId}' email"); }
            
            var claims = await userManager.GetClaimsAsync(user);
            return Ok(claims);
        }

        [HttpPost("user-claims")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AddUserClaims([FromBody] ManageUserClaimsDTO model)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid Request model."); }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
            { return BadRequest($"No user exits with '{model.Email}' email"); }

            var claims = model.Claims.Select(c => new Claim(c.Key, c.Value));

            var result = await userManager.AddClaimsAsync(user, claims);
            return Ok(result);
        }

        [HttpDelete("user-claims")]
        [Authorize(Roles = Role.SuperAdmin)]
        public async Task<IActionResult> DeleteUserClaims([FromBody] ManageUserClaimsDTO model)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid Request model."); }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
            { return BadRequest($"No user exits with '{model.Email}' email"); }

            var claims = model.Claims.Select(c => new Claim(c.Key, c.Value));

            var result = userManager.RemoveClaimsAsync(user, claims);
            return Ok(result);
        }


        #endregion


        #region Manage Role's Claims

        [HttpGet("role-claims")]
        [Authorize(Roles = Role.Admin )]
        public async Task<IActionResult> GetRoleClaims()
        {
            var roleClaims = new Dictionary<IdentityRole, IEnumerable<Claim>>();
            var currentRoles = roleManager.Roles.ToList();
            foreach (var role in currentRoles)
            {
                roleClaims.Add(role, await roleManager.GetClaimsAsync(role));
            }

            return Ok(roleClaims);
        }

        [HttpPost("role-claims")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> AddRoleClaims([FromBody] ManageRoleClaimsDTO manageRoleClaimsDTOs)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid Request model."); }

            string id = manageRoleClaimsDTOs.RoleId.ToString();
            var role = await roleManager.FindByIdAsync(id);

            if(role is null)
            {
                return BadRequest($"Role with give id '{id}' does not exits.");
            }

            var claims = manageRoleClaimsDTOs.Claims.Select(c => new Claim(c.Key, c.Value));

            var response = ManageRoleClaims(role, claims, roleManager.AddClaimAsync);

            return Ok(response);
        }

        [HttpDelete("role-claims")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteRoleClaims([FromBody] ManageRoleClaimsDTO manageRoleClaimsDTOs)
        {
            if (!ModelState.IsValid)
            { return BadRequest("Invalid Request model."); }

            string id = manageRoleClaimsDTOs.RoleId.ToString();
            var role = await roleManager.FindByIdAsync(id);

            if (role is null)
            {
                return BadRequest($"Role with give id '{id}' does not exits.");
            }

            var claims = manageRoleClaimsDTOs.Claims.Select(c => new Claim(c.Key, c.Value));

            var response = ManageRoleClaims(role, claims, roleManager.RemoveClaimAsync);

            return Ok(response);
        }

        private async Task<IEnumerable<StatusDTO<string, bool>>> ManageRoleClaims(IdentityRole role, IEnumerable<Claim> claims, Func<IdentityRole, Claim, Task<IdentityResult>> operation)
        {
            var response = new List<StatusDTO<string, bool>>();

            foreach (var claim in claims)
            {
                var status = new StatusDTO<string, bool>
                {
                    Id = role.Id,
                    Name = claim.Type
                };

                var result = await operation(role, claim);
                status.Status = result.Succeeded;
                status.Error = string.Join('|', result.Errors.Select(e => e.Description));
            }

            return response;
        }
        #endregion

        #region Manage Users


        [HttpGet("users")]
        [Authorize(Policy = "canViewUsers")]
        public IActionResult GetUsers()
        {
            var users = userManager.Users
                        .Select(u => new 
                        {   u.Email,
                            u.EmailConfirmed
                        });
            return Ok(users);
        }

        #endregion

    }
}