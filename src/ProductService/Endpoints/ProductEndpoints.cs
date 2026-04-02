using ProductService.Contracts;
using ProductService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products").WithTags("Product");

        products.MapGet(string.Empty, async (
            [FromQuery] string? ids,
            [FromQuery] string? categoryId,
            [FromQuery] string? name,
            IProductService productService,
            CancellationToken ct) =>
        {
            var hasIds = !string.IsNullOrWhiteSpace(ids);
            var hasCategoryId = !string.IsNullOrWhiteSpace(categoryId);
            var hasName = !string.IsNullOrWhiteSpace(name);

            if (!hasIds && !hasCategoryId && !hasName)
            {
                return Results.BadRequest("At least one query filter is required: ids, categoryId or name.");
            }

            if (hasIds && (hasCategoryId || hasName))
            {
                return Results.BadRequest("When ids is informed it must be the only filter.");
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
                var byCategory = await productService.GetAllByCategoryAsync(categoryId!, ct);
                var filtered = byCategory
                    .Where(product => product.Name.Contains(name!, StringComparison.OrdinalIgnoreCase));
                return Results.Ok(filtered);
            }

            if (hasCategoryId)
            {
                var byCategory = await productService.GetAllByCategoryAsync(categoryId!, ct);
                return Results.Ok(byCategory);
            }

            var byName = await productService.SearchByNameAsync(name!, ct);
            return Results.Ok(byName);
        })
        .WithName("GetProducts")
        .WithSummary("Get products by query filters");

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
