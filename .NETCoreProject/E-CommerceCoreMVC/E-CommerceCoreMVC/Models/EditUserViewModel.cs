using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_CommerceCoreMVC.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleId { get; set; } // ID của vai trò hiện tại
        public string RoleName { get; set; } // Tên của vai trò hiện tại
        public List<SelectListItem> Roles { get; set; } // Danh sách vai trò để chọn
    }


}
