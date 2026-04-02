namespace ProductService.Data.Entities;

public class CategoryEntity
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public List<ProductEntity> Products { get; set; } = [];
}
