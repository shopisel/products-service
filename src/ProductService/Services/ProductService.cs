using ProductService.Contracts;
using ProductService.Data;
using ProductService.Data.Entities;

namespace ProductService.Services;

public partial class ProductService : IProductService
{
    private readonly ProductServiceDbContext _dbContext;

    public ProductService(ProductServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static ProductResponse MapToResponse(ProductEntity entity)
    {
        return new ProductResponse(
            entity.Id,
            entity.Name,
            entity.Barcode,
            entity.CategoryId);
    }

    private static string GenerateProductId()
    {
        return $"prod_{Guid.NewGuid():N}";
    }
}
