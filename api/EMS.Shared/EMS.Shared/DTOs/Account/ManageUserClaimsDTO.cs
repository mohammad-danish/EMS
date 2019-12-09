using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMS.Shared.DTOs.Account
{
    public class ManageUserClaimsDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public IDictionary<string, string> Claims { get; set; }

    }
}
