using Microsoft.EntityFrameworkCore;

namespace ProductService.Services;

public partial class ProductService
{
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(currentProduct => currentProduct.Id == id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
