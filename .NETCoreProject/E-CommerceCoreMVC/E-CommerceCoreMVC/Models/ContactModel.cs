using E_CommerceCoreMVC.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_CommerceCoreMVC.Models
{
    public class ContactModel
    {
        [Key]
        [Required(ErrorMessage = "Yêu Cầu Nhập Tiêu Đề Website")]
        public string Name { get; set; } 
        [Required(ErrorMessage = "Yêu Cầu Nhập Bản Đồ")]
        public string Map { get; set; }
        [Required(ErrorMessage = "Yêu Cầu Nhập Số Điện Thoại")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Yêu Cầu Nhập Email")]
        public string Email { get; set; }

        [Required (ErrorMessage = "Yêu Cầu Nhập Thông Tin Liên Hệ")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
        public string LogoImage {  get; set; }  
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
