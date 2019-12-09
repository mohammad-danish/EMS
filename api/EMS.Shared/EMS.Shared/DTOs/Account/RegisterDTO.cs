using System.ComponentModel.DataAnnotations;

namespace EMS.Shared.DTOs.Account
{
    public class RegisterDTO : LoginDTO       
    {
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}
