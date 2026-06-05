using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Persistence;

public sealed class CatalogDbContextFactory
    : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<CatalogDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=bisa_shop;Username=postgres;Password=postgres");

        return new CatalogDbContext(optionsBuilder.Options);
    }
}