namespace CatalogService.Persistence.Products;

public class ProductRepository(CatalogDbContext context) : IProductRepository
{
    public Task<Product?> FindAsync(Guid id, CancellationToken ct) =>
        Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Add(Product product)
    {
        Products.Add(product);
    }

    private DbSet<Product> Products => context.Products;
}