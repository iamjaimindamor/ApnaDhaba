namespace ApnaDhaba.Api_Models;

public partial class ProductTable
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public int Price { get; set; }

    public string? ImageUrl { get; set; }

    public int? CategoryId1 { get; set; }

    public virtual CategoryTable? CategoryId1Navigation { get; set; }
}