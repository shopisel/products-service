namespace ProductService.Contracts;

public sealed record ProductResponse(
    string Id,
    string Name,
    string Image
    );
