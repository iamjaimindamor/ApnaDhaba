namespace ApnaDhaba.Models
{
    public class DualDetailModel
    {
        public Order order { get; set; }

        public Product product { get; set; }

        public List<Order> orders { get; set; }
        public List<Product> products { get; set; }
    }
}