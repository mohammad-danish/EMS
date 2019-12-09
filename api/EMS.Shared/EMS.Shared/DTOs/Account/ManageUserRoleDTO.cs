using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMS.Shared.DTOs.Account
{
    public class ManageUserRoleDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(1)]
        public IEnumerable<string> Roles { get; set; }
    }
}
