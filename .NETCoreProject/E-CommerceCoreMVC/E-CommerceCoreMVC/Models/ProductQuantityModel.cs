using System.ComponentModel.DataAnnotations;

namespace E_CommerceCoreMVC.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Vui lòng nhập số lượng sản phẩm")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}