using Microsoft.AspNetCore.Identity;

namespace E_CommerceCoreMVC.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Ocupation { get; set; }
        public string RoleId { get; set; }
        public string Token { get; set; }
    }
}
