namespace ApnaDhaba.Api_Models;

public partial class CategoryTable
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<ProductTable> ProductTables { get; set; } = new List<ProductTable>();
}