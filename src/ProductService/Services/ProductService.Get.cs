using ProductService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    public async Task<IEnumerable<ProductResponse>> GetByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (idList.Count == 0)
        {
            return [];
        }

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => idList.Contains(product.Id))
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllByCategoryAsync(
        string categoryId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCategoryId = categoryId.Trim();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.CategoryId == normalizedCategoryId)
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> SearchByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Name.ToLower().Contains(normalizedName))
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }
}
