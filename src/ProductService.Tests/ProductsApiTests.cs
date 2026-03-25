using System.Net;
using System.Net.Http.Json;
using ProductService.Data;
using ProductService.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Tests;

public class ProductsApiTests(ProductServiceApiFactory factory) : IClassFixture<ProductServiceApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static void SeedProducts(ProductServiceApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        dbContext.Products.AddRange(
            new ProductEntity { Id = "prod_1", Name = "Leite UHT", Image = "leite.png" },
            new ProductEntity { Id = "prod_2", Name = "Cereais", Image = "cereais.png" },
            new ProductEntity { Id = "prod_3", Name = "Sumo", Image = "sumo.png" }
        );
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetProductsByIds_ReturnsMatchingProducts()
    {
        SeedProducts(factory);

        var response = await _client.GetAsync("/products?ids=prod_1,prod_2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(products);
        Assert.Equal(2, products!.Count);
        Assert.Contains(products, p => p.Id == "prod_1" && p.Name == "Leite UHT");
        Assert.Contains(products, p => p.Id == "prod_2" && p.Name == "Cereais");
    }

    [Fact]
    public async Task GetProductsByIds_WithMissingIds_ReturnsOnlyFound()
    {
        SeedProducts(factory);

        var response = await _client.GetAsync("/products?ids=prod_1,nonexistent");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        Assert.NotNull(products);
        Assert.Single(products!);
        Assert.Equal("prod_1", products![0].Id);
    }

    [Fact]
    public async Task GetProductsByIds_WithoutIds_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/products");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record ProductResponse(string Id, string Name, string Image);
}