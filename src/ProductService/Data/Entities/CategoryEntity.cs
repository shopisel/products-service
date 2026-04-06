namespace ProductService.Data.Entities;

public class CategoryEntity
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string? ParentCategoryId { get; set; }

    public CategoryEntity? ParentCategory { get; set; }

    public List<CategoryEntity> Subcategories { get; set; } = [];

    public List<ProductEntity> Products { get; set; } = [];
}
