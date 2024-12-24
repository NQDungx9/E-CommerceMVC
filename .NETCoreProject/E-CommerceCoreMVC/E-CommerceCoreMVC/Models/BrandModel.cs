using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(10, ErrorMessage = "Yêu Cầu Nhập Tên Thương Hiệu")]
        public string Name { get; set; }
        [Required, MaxLength(255, ErrorMessage = "Yêu Cầu Nhập Mô Tả Thương Hiệu")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }

    }
}
