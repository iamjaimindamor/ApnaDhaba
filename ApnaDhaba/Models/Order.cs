namespace ApnaDhaba.Models;

public partial class Order
{
    public int OrederDetailsId { get; set; }

    public string? OrderNo { get; set; }

    public int? ProductId { get; set; }

    public int? Quantity { get; set; }
    public string? Username { get; set; }

    public string? Status { get; set; }

    public int? PaymentId { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual Payment? Payment { get; set; }

    public virtual Product? Product { get; set; }
}