using ProductService.Contracts;
using ProductService.Data;
using ProductService.Data.Entities;

namespace ProductService.Services;

public partial class CategoryService : ICategoryService
{
    private readonly ProductServiceDbContext _dbContext;

    public CategoryService(ProductServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static CategoryResponse MapToResponse(CategoryEntity entity)
    {
        return new CategoryResponse(entity.Id, entity.Name, entity.Image);
    }

    private static string GenerateCategoryId()
    {
        return $"cat_{Guid.NewGuid():N}";
    }
}
