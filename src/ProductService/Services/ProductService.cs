using ProductService.Contracts;
using ProductService.Data;
using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly ProductServiceDbContext _dbContext;

    public ProductService(ProductServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ProductResponse>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => idList.Contains(product.Id))
            .ToListAsync(cancellationToken);

        return products.Select(MapToResponse);
    }

    private static ProductResponse MapToResponse(ProductEntity entity)
    {
        return new ProductResponse(
            entity.Id,
            entity.Name,
            entity.Image);
    }
}
