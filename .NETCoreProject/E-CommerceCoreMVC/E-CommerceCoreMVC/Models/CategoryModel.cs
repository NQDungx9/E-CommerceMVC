using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(10, ErrorMessage = "Yêu Cầu Nhập Tên Danh Mục")]
        public string Name { get; set; }
        [Required, MaxLength(255, ErrorMessage = "Yêu Cầu Nhập Mô Tả Danh Mục")]
        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }
    }
}
