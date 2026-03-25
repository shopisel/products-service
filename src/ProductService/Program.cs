using ProductService.Data;
using ProductService.Endpoints;
using ProductService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("ProductService");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'ConnectionStrings:ProductService' is required.");
}

builder.Services.AddDbContext<ProductServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Register custom services
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await InitializeDatabaseAsync(app);

// Map Endpoint slices
app.MapProductEndpoints();

await app.RunAsync();

static async Task InitializeDatabaseAsync(WebApplication application)
{
    using var scope = application.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
    if (dbContext.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
    {
        await dbContext.Database.MigrateAsync();
        return;
    }

    await dbContext.Database.EnsureCreatedAsync();
}

public partial class Program;
