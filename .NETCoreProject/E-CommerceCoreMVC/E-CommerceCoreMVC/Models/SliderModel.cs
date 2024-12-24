using E_CommerceCoreMVC.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_CommerceCoreMVC.Models
{
    public class SliderModel
    {
        [Key]   
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu Cầu Nhập Tên Slider")]
        public string Name { get; set; }
        [Required, MaxLength(255, ErrorMessage = "Yêu Cầu Nhập Mô Tả mô tả")]
        public string Description { get; set; }
        public int Status { get; set; }
        public string Image {  get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
