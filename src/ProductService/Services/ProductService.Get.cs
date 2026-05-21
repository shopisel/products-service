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
        int? skip = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCategoryId = categoryId.Trim();

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.CategoryId == normalizedCategoryId)
            .OrderBy(product => product.Name)
            .AsQueryable();

        if (skip is > 0)
        {
            query = query.Skip(skip.Value);
        }

        if (limit is > 0)
        {
            query = query.Take(limit.Value);
        }

        var products = await query.ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> SearchByNameAsync(
        string name,
        int? skip = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Name.ToLower().Contains(normalizedName))
            .OrderBy(product => product.Name)
            .AsQueryable();

        if (skip is > 0)
        {
            query = query.Skip(skip.Value);
        }

        if (limit is > 0)
        {
            query = query.Take(limit.Value);
        }

        var products = await query.ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }

    public async Task<IEnumerable<ProductResponse>> SearchByCategoryAndNameAsync(
        string categoryId,
        string name,
        int? skip = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedCategoryId = categoryId.Trim();
        var normalizedName = name.Trim().ToLower();

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.CategoryId == normalizedCategoryId)
            .Where(product => product.Name.ToLower().Contains(normalizedName))
            .OrderBy(product => product.Name)
            .AsQueryable();

        if (skip is > 0)
        {
            query = query.Skip(skip.Value);
        }

        if (limit is > 0)
        {
            query = query.Take(limit.Value);
        }

        var products = await query.ToListAsync(cancellationToken);
        return products.Select(MapToResponse);
    }
}
