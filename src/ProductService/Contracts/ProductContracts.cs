namespace ProductService.Contracts;

public sealed record CategoryResponse(
    string Id,
    string Name,
    string Image,
    string? ParentCategoryId);

public sealed record CreateCategoryRequest(
    string Name,
    string Image,
    string? ParentCategoryId);

public sealed record UpdateCategoryRequest(
    string? Name,
    string? Image,
    string? ParentCategoryId);

public sealed record ProductResponse(
    string Id,
    string Name,
    string? Brand,
    string Barcode,
    string Image,
    string CategoryId);

public sealed record CreateProductRequest(
    string Name,
    string? Brand,
    string Barcode,
    string? Image,
    string CategoryId);

public sealed record UpdateProductRequest(
    string? Name,
    string? Brand,
    string? Barcode,
    string? Image,
    string? CategoryId);
