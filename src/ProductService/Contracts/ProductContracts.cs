namespace ProductService.Contracts;

public sealed record CategoryResponse(
    string Id,
    string Name,
    string Image);

public sealed record CreateCategoryRequest(
    string Name,
    string Image);

public sealed record UpdateCategoryRequest(
    string? Name,
    string? Image);

public sealed record ProductResponse(
    string Id,
    string Name,
    string Barcode,
    string CategoryId);

public sealed record CreateProductRequest(
    string Name,
    string Barcode,
    string CategoryId);

public sealed record UpdateProductRequest(
    string? Name,
    string? Barcode,
    string? CategoryId);
