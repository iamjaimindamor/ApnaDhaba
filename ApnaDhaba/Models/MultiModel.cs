using ApnaDhaba.Api_Models;

namespace ApnaDhaba.Models
{
    public class MultiModel
    {
        public List<Category>? Categories { get; set; }
        public List<Product>? Products { get; set; }
        public List<Cart> cart { get; set; }
        public Cart singlecart { get; set; }
        public UserModel User { get; set; }
        public dualModel dualModel { get; set; }
        public int? updateQuantity { get; set; }

        public decimal? subtotal { get; set; }
        public int? total { get; set; } = 0;
    }
}