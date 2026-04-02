using ProductService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class CategoryService
{
    public async Task<CategoryResponse?> UpdateAsync(
        string id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(currentCategory => currentCategory.Id == id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("The category name cannot be empty.", nameof(request));
            }

            category.Name = request.Name.Trim();
        }

        if (request.Image is not null)
        {
            category.Image = request.Image.Trim();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(category);
    }
}
