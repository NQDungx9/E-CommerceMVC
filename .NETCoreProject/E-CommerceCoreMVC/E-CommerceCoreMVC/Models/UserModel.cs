using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models
{
	public class UserModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập UserName")]
		public string UserName { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập Email"), EmailAddress]
		public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Email"), Phone]
        public string PhoneNumber { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập PassWord")]
		public string Password { get; set; }

	}
}
