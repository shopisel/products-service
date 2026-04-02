using System.Net;
using System.Net.Http.Json;
using ProductService.Contracts;
using ProductService.Data;
using ProductService.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Tests;

public class ProductsApiTests(ProductServiceApiFactory factory) : IClassFixture<ProductServiceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static void ResetDatabase(ProductServiceApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    private static void SeedBaseData(ProductServiceApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        dbContext.Categories.AddRange(
            new CategoryEntity { Id = "cat_food", Name = "Food", Image = "food.png" },
            new CategoryEntity { Id = "cat_drinks", Name = "Drinks", Image = "drinks.png" });

        dbContext.Products.AddRange(
            new ProductEntity { Id = "prod_1", Name = "Leite UHT", Barcode = "560123", CategoryId = "cat_food" },
            new ProductEntity { Id = "prod_2", Name = "Cereais", Barcode = "560456", CategoryId = "cat_food" },
            new ProductEntity { Id = "prod_3", Name = "Sumo de Laranja", Barcode = "560789", CategoryId = "cat_drinks" });

        dbContext.SaveChanges();
    }

    [Fact]
    public async Task CategoriesCrudAndDeleteBlocking_WorksAsExpected()
    {
        ResetDatabase(factory);

        var createResponse = await _client.PostAsJsonAsync("/categories", new
        {
            name = "Mercearia",
            image = "groceries.png"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(createdCategory);
        Assert.StartsWith("cat_", createdCategory!.Id);

        var updateCategoryResponse = await _client.PutAsJsonAsync($"/categories/{createdCategory.Id}", new
        {
            name = "Mercearia e Casa",
            image = "home.png"
        });
        Assert.Equal(HttpStatusCode.OK, updateCategoryResponse.StatusCode);

        var getCategoriesResponse = await _client.GetAsync("/categories");
        Assert.Equal(HttpStatusCode.OK, getCategoriesResponse.StatusCode);

        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(categories);
        Assert.Contains(categories!, category => category.Id == createdCategory.Id && category.Name == "Mercearia e Casa");

        var createProductResponse = await _client.PostAsJsonAsync("/products", new
        {
            name = "Arroz",
            barcode = "560001",
            categoryId = createdCategory.Id
        });
        Assert.Equal(HttpStatusCode.Created, createProductResponse.StatusCode);

        var createdProduct = await createProductResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);

        var blockedDeleteResponse = await _client.DeleteAsync($"/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.Conflict, blockedDeleteResponse.StatusCode);

        var deleteProductResponse = await _client.DeleteAsync($"/products/{createdProduct!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteProductResponse.StatusCode);

        var deleteCategoryResponse = await _client.DeleteAsync($"/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteCategoryResponse.StatusCode);
    }

    [Fact]
    public async Task ProductsCrudFlow_WorksEndToEnd()
    {
        SeedBaseData(factory);

        var createResponse = await _client.PostAsJsonAsync("/products", new
        {
            name = "Iogurte Natural",
            barcode = "560777",
            categoryId = "cat_food"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(createdProduct);
        Assert.StartsWith("prod_", createdProduct!.Id);

        var getByIdsResponse = await _client.GetAsync($"/products?ids={createdProduct.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdsResponse.StatusCode);

        var getByIds = await getByIdsResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(getByIds);
        Assert.Single(getByIds!);
        Assert.Equal("Iogurte Natural", getByIds[0].Name);

        var updateResponse = await _client.PutAsJsonAsync($"/products/{createdProduct.Id}", new
        {
            name = "Iogurte Grego",
            barcode = "560888",
            categoryId = "cat_drinks"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedProduct = await updateResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(updatedProduct);
        Assert.Equal("Iogurte Grego", updatedProduct!.Name);
        Assert.Equal("560888", updatedProduct.Barcode);
        Assert.Equal("cat_drinks", updatedProduct.CategoryId);

        var deleteResponse = await _client.DeleteAsync($"/products/{createdProduct.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetProductsByFilters_ReturnsExpectedResults()
    {
        SeedBaseData(factory);

        var byCategoryResponse = await _client.GetAsync("/products?categoryId=cat_food");
        Assert.Equal(HttpStatusCode.OK, byCategoryResponse.StatusCode);

        var byCategory = await byCategoryResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(byCategory);
        Assert.Equal(2, byCategory!.Count);
        Assert.All(byCategory, product => Assert.Equal("cat_food", product.CategoryId));

        var byNameResponse = await _client.GetAsync("/products?name=LEITE");
        Assert.Equal(HttpStatusCode.OK, byNameResponse.StatusCode);

        var byName = await byNameResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(byName);
        Assert.Single(byName!);
        Assert.Equal("prod_1", byName[0].Id);

        var byCategoryAndNameResponse = await _client.GetAsync("/products?categoryId=cat_food&name=cer");
        Assert.Equal(HttpStatusCode.OK, byCategoryAndNameResponse.StatusCode);

        var byCategoryAndName = await byCategoryAndNameResponse.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(byCategoryAndName);
        Assert.Single(byCategoryAndName!);
        Assert.Equal("prod_2", byCategoryAndName[0].Id);
    }

    [Fact]
    public async Task GetProducts_WithoutFilterOrWithInvalidCombination_ReturnsBadRequest()
    {
        SeedBaseData(factory);

        var withoutFilterResponse = await _client.GetAsync("/products");
        Assert.Equal(HttpStatusCode.BadRequest, withoutFilterResponse.StatusCode);

        var idsWithAnotherFilterResponse = await _client.GetAsync("/products?ids=prod_1&name=leite");
        Assert.Equal(HttpStatusCode.BadRequest, idsWithAnotherFilterResponse.StatusCode);
    }
}
