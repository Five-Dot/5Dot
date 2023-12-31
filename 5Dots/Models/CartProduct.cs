

using System.ComponentModel.DataAnnotations;

namespace _5Dots.Models
{
    public class CartProduct
    {
        [Key]
        public int CartId { get; set; }
        public Cart Cart{ get; set; }
        [Required(ErrorMessage = "Cart Quantity is required")]
        [Range(1,int.MaxValue)]
        public int ProductQuantity { get; set; }
        [Key]
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}
