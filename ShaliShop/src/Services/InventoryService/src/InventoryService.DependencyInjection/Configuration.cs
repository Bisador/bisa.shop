using InventoryService.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.DependencyInjection;

public static class Configuration
{
    public static IServiceCollection AddInventoryService(this IServiceCollection services, string connectionString)
    {
        //services.AddScoped<IInventoryReadRepository, InventoryReadRepository>();
        //services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<InventoryGetAllQuery>());
        
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(AssemblyReference.GetAssemblyReference.FullName)));

        return services;
    }
}