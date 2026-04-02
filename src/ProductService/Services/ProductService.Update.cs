using ProductService.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    public async Task<ProductResponse?> UpdateAsync(
        string id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("The product name cannot be empty.", nameof(request));
            }

            product.Name = request.Name.Trim();
        }

        if (request.Barcode is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Barcode))
            {
                throw new ArgumentException("The product barcode cannot be empty.", nameof(request));
            }

            product.Barcode = request.Barcode.Trim();
        }

        if (request.CategoryId is not null)
        {
            if (string.IsNullOrWhiteSpace(request.CategoryId))
            {
                throw new ArgumentException("The categoryId cannot be empty.", nameof(request));
            }

            var categoryId = request.CategoryId.Trim();
            var categoryExists = await _dbContext.Categories
                .AnyAsync(category => category.Id == categoryId, cancellationToken);

            if (!categoryExists)
            {
                throw new ArgumentException("The informed category does not exist.", nameof(request));
            }

            product.CategoryId = categoryId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(product);
    }
}
