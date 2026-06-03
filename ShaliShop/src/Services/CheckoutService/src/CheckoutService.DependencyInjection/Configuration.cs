using CheckoutService.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CheckoutService.DependencyInjection;

public static class Configuration
{
    public static IServiceCollection AddCheckoutService(this IServiceCollection services, string connectionString)
    { 
        services.AddDbContext<CheckoutDbContext>(options =>
            options.UseNpgsql(connectionString, 
                b => b.MigrationsAssembly(AssemblyReference.GetAssemblyReference.FullName)));

        return services;
    }
}