namespace ProductService.Data.Entities;

public class ProductEntity
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string CategoryId { get; set; } = string.Empty;

    public CategoryEntity? Category { get; set; }
}
