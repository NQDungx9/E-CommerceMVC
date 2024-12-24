using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models.ViewModel
{
    public class LoginViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập UserName")]
        public string UserName { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập PassWord")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
