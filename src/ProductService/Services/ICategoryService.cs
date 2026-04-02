using ProductService.Contracts;

namespace ProductService.Services;

public enum DeleteCategoryResult
{
    Success,
    NotFound,
    HasProducts
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse?> UpdateAsync(string id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<DeleteCategoryResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
