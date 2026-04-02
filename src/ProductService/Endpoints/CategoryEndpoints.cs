using ProductService.Contracts;
using ProductService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProductService.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var categories = app.MapGroup("/categories").WithTags("Category");

        categories.MapGet(string.Empty, async (ICategoryService categoryService, CancellationToken ct) =>
        {
            var result = await categoryService.GetAllAsync(ct);
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("Get all categories");

        categories.MapPost(string.Empty, async (
            [FromBody] CreateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken ct) =>
        {
            try
            {
                var createdCategory = await categoryService.CreateAsync(request, ct);
                return Results.Created($"/categories/{createdCategory.Id}", createdCategory);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("CreateCategory")
        .WithSummary("Create category");

        categories.MapPut("/{categoryId}", async (
            string categoryId,
            [FromBody] UpdateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken ct) =>
        {
            try
            {
                var updatedCategory = await categoryService.UpdateAsync(categoryId, request, ct);
                return updatedCategory is null ? Results.NotFound() : Results.Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "error"] = [ex.Message]
                });
            }
        })
        .WithName("UpdateCategory")
        .WithSummary("Update category");

        categories.MapDelete("/{categoryId}", async (
            string categoryId,
            ICategoryService categoryService,
            CancellationToken ct) =>
        {
            var result = await categoryService.DeleteAsync(categoryId, ct);
            return result switch
            {
                DeleteCategoryResult.Success => Results.NoContent(),
                DeleteCategoryResult.HasProducts => Results.Conflict(new
                {
                    message = "Category cannot be deleted because it still has products."
                }),
                _ => Results.NotFound()
            };
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete category");
    }
}
