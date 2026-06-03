using CatalogService.Application;
using CatalogService.Application.Abstraction;
using CatalogService.Application.Abstraction.Products;
using CatalogService.Domain.Products.Repository;
using CatalogService.Persistence;
using CatalogService.Persistence.Products;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.DependencyInjection;

public static class Configuration
{
    public static IServiceCollection AddCatalogService(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(CatalogServiceApplicationAssemblyReference.Get()));

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(CatalogServicePersistenceAssemblyReference.GetAssemblyReference.FullName)));

        return services;
    }
}