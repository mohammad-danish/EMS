using System;
using System.Collections.Generic;

namespace EMS.Shared.DTOs.Account
{
    public class ManageRoleClaimsDTO
    {
        public Guid RoleId { get; set; }
        public IDictionary<string, string> Claims { get; set; }
    }
}
