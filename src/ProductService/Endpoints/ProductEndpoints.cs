using ProductService.Contracts;
using ProductService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Endpoints;

public static class ProductEndpoints
{
    private const int MaxProductsPageSize = 200;

    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products").WithTags("Product");

        products.MapGet(string.Empty, async (
            [FromQuery] string? ids,
            [FromQuery] string? categoryId,
            [FromQuery] string? name,
            [FromQuery] int? skip,
            [FromQuery] int? limit,
            IProductService productService,
            CancellationToken ct) =>
        {
            var hasIds = !string.IsNullOrWhiteSpace(ids);
            var hasCategoryId = !string.IsNullOrWhiteSpace(categoryId);
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasSkip = skip.HasValue;
            var hasLimit = limit.HasValue;

            if (!hasIds && !hasCategoryId && !hasName)
            {
                return Results.BadRequest("At least one query filter is required: ids, categoryId or name.");
            }

            if (hasIds && (hasCategoryId || hasName))
            {
                return Results.BadRequest("When ids is informed it must be the only filter.");
            }

            if (hasSkip && !hasLimit)
            {
                return Results.BadRequest("limit is required when skip is informed.");
            }

            if (skip is < 0)
            {
                return Results.BadRequest("skip must be greater than or equal to zero.");
            }

            if (limit is <= 0)
            {
                return Results.BadRequest("limit must be greater than zero.");
            }

            if (limit is > MaxProductsPageSize)
            {
                return Results.BadRequest($"limit must be less than or equal to {MaxProductsPageSize}.");
            }

            if (hasIds)
            {
                var idList = ids!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct()
                    .ToList();

                if (idList.Count == 0)
                {
                    return Results.BadRequest("Invalid ids format.");
                }

                var byIds = await productService.GetByIdsAsync(idList, ct);
                return Results.Ok(byIds);
            }

            if (hasCategoryId && hasName)
            {
                var filtered = await productService.SearchByCategoryAndNameAsync(categoryId!, name!, skip, limit, ct);
                return Results.Ok(filtered);
            }

            if (hasCategoryId)
            {
                var byCategory = await productService.GetAllByCategoryAsync(categoryId!, skip, limit, ct);
                return Results.Ok(byCategory);
            }

            var byName = await productService.SearchByNameAsync(name!, skip, limit, ct);
            return Results.Ok(byName);
        })
        .WithName("GetProducts")
        .WithSummary("Get products by query filters");

        products.MapGet("/related", async (
            [FromQuery] string? favoriteIds,
            [FromQuery] int? limit,
            [FromQuery] int? maxDistance,
            IProductService productService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(favoriteIds))
            {
                return Results.BadRequest("favoriteIds is required.");
            }

            var idList = favoriteIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct()
                .ToList();

            if (idList.Count == 0)
            {
                return Results.BadRequest("Invalid favoriteIds format.");
            }

            var requestedLimit = limit ?? 10;
            if (requestedLimit <= 0)
            {
                return Results.BadRequest("limit must be greater than zero.");
            }

            if (maxDistance is <= 0)
            {
                return Results.BadRequest("maxDistance must be greater than zero.");
            }

            var relatedProducts = await productService.GetRelatedByFavoriteIdsAsync(
                idList,
                requestedLimit,
                maxDistance,
                ct);

            return Results.Ok(relatedProducts);
        })
        .WithName("GetRelatedProductsByFavorites")
        .WithSummary("Get related products based on favorite product ids");

        products.MapPost(string.Empty, async (
            [FromBody] CreateProductRequest request,
            IProductService productService,
            CancellationToken ct) =>
        {
            try
            {
                var createdProduct = await productService.CreateAsync(request, ct);
                return Results.Created($"/products/{createdProduct.Id}", createdProduct);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("CreateProduct")
        .WithSummary("Create product");

        products.MapPost("/qrCode", async (
            [FromBody] QrCodeLookupRequest request,
            IProductService productService,
            CancellationToken ct) =>
        {
            if (request is null)
            {
                return Results.BadRequest("Request body is required.");
            }

            var hasBarcode = !string.IsNullOrWhiteSpace(request.Barcode);
            var hasKeywords = request.Keywords is { Count: > 0 } &&
                              request.Keywords.Any(keyword => !string.IsNullOrWhiteSpace(keyword));

            if (!hasBarcode && !hasKeywords)
            {
                return Results.BadRequest("At least one lookup input is required: barcode or keywords.");
            }

            var lookup = await productService.LookupByQrCodeAsync(request, ct);
            return Results.Ok(lookup);
        })
        .WithName("LookupProductByQrCode")
        .WithSummary("Lookup product by barcode/keywords (exact match or related suggestions)");

        products.MapPut("/{productId}", async (
            string productId,
            [FromBody] UpdateProductRequest request,
            IProductService productService,
            CancellationToken ct) =>
        {
            try
            {
                var updatedProduct = await productService.UpdateAsync(productId, request, ct);
                return updatedProduct is null ? Results.NotFound() : Results.Ok(updatedProduct);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("UpdateProduct")
        .WithSummary("Update product");

        products.MapDelete("/{productId}", async (
            string productId,
            IProductService productService,
            CancellationToken ct) =>
        {
            var success = await productService.DeleteAsync(productId, ct);
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete product");
    }
}
