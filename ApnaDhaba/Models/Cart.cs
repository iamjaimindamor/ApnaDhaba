namespace ApnaDhaba.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? ProductId { get; set; }
    public int? BasketID { get; set; }

    public decimal? Quantity { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Username { get; set; }
    public virtual Product? Product { get; set; }
}