using ProductService.Contracts;
using ProductService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("The product name is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Barcode))
        {
            throw new ArgumentException("The product barcode is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.CategoryId))
        {
            throw new ArgumentException("The categoryId is required.", nameof(request));
        }

        var categoryId = request.CategoryId.Trim();
        var categoryExists = await _dbContext.Categories
            .AnyAsync(category => category.Id == categoryId, cancellationToken);

        if (!categoryExists)
        {
            throw new ArgumentException("The informed category does not exist.", nameof(request));
        }

        var product = new ProductEntity
        {
            Id = GenerateProductId(),
            Name = request.Name.Trim(),
            Barcode = request.Barcode.Trim(),
            CategoryId = categoryId
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(product);
    }
}
