using ProductService.Contracts;
using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

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

        var parentCategoryId = string.IsNullOrWhiteSpace(request.ParentCategoryId)
            ? null
            : request.ParentCategoryId.Trim();

        if (parentCategoryId is not null)
        {
            var parentExists = await _dbContext.Categories
                .AnyAsync(category => category.Id == parentCategoryId, cancellationToken);

            if (!parentExists)
            {
                throw new ArgumentException("The informed parent category does not exist.", nameof(request));
            }
        }

        var category = new CategoryEntity
        {
            Id = GenerateCategoryId(),
            Name = request.Name.Trim(),
            Image = request.Image?.Trim() ?? string.Empty,
            ParentCategoryId = parentCategoryId
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(category);
    }
}
