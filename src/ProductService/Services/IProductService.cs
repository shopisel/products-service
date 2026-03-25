using ProductService.Contracts;

namespace ProductService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
}
