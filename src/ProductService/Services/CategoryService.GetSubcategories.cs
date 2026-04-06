using ProductService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class CategoryService
{
    public async Task<IEnumerable<CategoryResponse>?> GetSubcategoriesAsync(
        string parentCategoryId,
        CancellationToken cancellationToken = default)
    {
        var trimmedParentId = parentCategoryId.Trim();

        if (string.IsNullOrWhiteSpace(trimmedParentId))
        {
            return null;
        }

        var parentExists = await _dbContext.Categories
            .AnyAsync(category => category.Id == trimmedParentId, cancellationToken);

        if (!parentExists)
        {
            return null;
        }

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.ParentCategoryId == trimmedParentId)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToResponse);
    }
}
