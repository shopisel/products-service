using ProductService.Services;

namespace ProductService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products").WithTags("Product");

        products.MapGet(string.Empty, async (string ids, IProductService productService, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return Results.BadRequest("O parâmetro 'ids' é obrigatório.");
            }

            var idList = ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();

            if (idList.Count == 0)
            {
                return Results.BadRequest("Formato de IDs inválido.");
            }

            var result = await productService.GetByIdsAsync(idList, ct);
            return Results.Ok(result);
        })
        .WithName("GetProductsByIds")
        .WithSummary("Obter detalhes de múltiplos produtos");
    }
}
