using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models.ViewModel
{
    public class ProductDetailsViewModel
    {
        public ProductModel ProductDetails { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Tên")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập đánh giá")]
        public string Comment { get; set; }
        public DateTime NgayDang { get; set; }
        public List<RatingModel> Comments { get; set; }
    }
}
