using E_CommerceCoreMVC.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_CommerceCoreMVC.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(10, ErrorMessage = "Yêu cầu nhập Tên sản phẩm")]
        public string Name { get; set; }
        [Required, MaxLength(255, ErrorMessage = "Yêu cầu nhập Mô Tả sản phẩm")]
        public string Description { get; set; }
        public string Slug { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá sản phẩm")]
        [Range (0.01, double.MaxValue)]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá vốn sản phẩm")]
        public decimal CapitalPrice { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]

        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public string Image { get; set; }
        public ICollection<RatingModel> Ratings { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; } 
        public CategoryModel Category { get; set; }
        public BrandModel Brand { get; set; }
    }
}
