using ProductService.Contracts;
using ProductService.Data.Entities;

namespace ProductService.Services;

public partial class CategoryService
{
    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("The category name is required.", nameof(request));
        }

        var category = new CategoryEntity
        {
            Id = GenerateCategoryId(),
            Name = request.Name.Trim(),
            Image = request.Image?.Trim() ?? string.Empty
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(category);
    }
}
