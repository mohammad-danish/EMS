using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMS.Shared.DTOs.Account
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string EmailId { get; set; }

        [Required]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\d\w])(?=.{8,16}$)")]
        public string Password { get; set; }

    }
}
