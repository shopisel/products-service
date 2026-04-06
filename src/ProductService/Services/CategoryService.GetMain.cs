using ProductService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class CategoryService
{
    public async Task<IEnumerable<CategoryResponse>> GetMainAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.ParentCategoryId == null)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToResponse);
    }
}
