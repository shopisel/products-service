using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class CategoryService
{
    public async Task<DeleteCategoryResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(currentCategory => currentCategory.Id == id, cancellationToken);

        if (category is null)
        {
            return DeleteCategoryResult.NotFound;
        }

        var hasProducts = await _dbContext.Products
            .AnyAsync(product => product.CategoryId == id, cancellationToken);

        if (hasProducts)
        {
            return DeleteCategoryResult.HasProducts;
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return DeleteCategoryResult.Success;
    }
}
