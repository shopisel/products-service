using ProductService.Contracts;

namespace ProductService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllByCategoryAsync(string categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponse>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponse>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse?> UpdateAsync(string id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
