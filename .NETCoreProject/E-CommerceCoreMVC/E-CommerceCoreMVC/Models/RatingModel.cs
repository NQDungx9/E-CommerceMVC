using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_CommerceCoreMVC.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get;set; }
        public int ProductId { get;set; }
        [Required(ErrorMessage = "Vui lòng nhập Tên")]
        public string Name { get;set; }
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get;set; }
        [Required(ErrorMessage = "Vui lòng nhập đánh giá")]
        public string Comment { get;set; }
        public string Star { get;set; }
        public DateTime NgayDang { get;set; }

        [ForeignKey("ProductId")]
        public ProductModel Product { get;set; }
    }
}
