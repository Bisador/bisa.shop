using System.Text.Json;
using Shared.Persistence.Converters;

namespace CatalogService.Persistence.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(128);
        builder.Property(p => p.Description).HasMaxLength(512);
        builder.Property(p => p.Category).IsRequired();
        builder.Property(p => p.IsPublished);
        builder.Property(p => p.IsDiscontinued);
        builder.Property(p => p.PublishedAt);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount");
            price.Property(p => p.Currency).HasColumnName("PriceCurrency");
        });

        builder.OwnsMany(p => p.Variants, variants =>
        {
            variants.WithOwner().HasForeignKey("ProductId");
            variants.Property(v => v.Sku).IsRequired();

            variants.Property<Dictionary<string, string>>("_options")
                .HasColumnName("Options")
                .HasConversion(new JsonValueConverter<Dictionary<string, string>>())
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            variants.OwnsOne(v => v.PriceOverride, overridePrice =>
            {
                overridePrice.Property(p => p.Amount).HasColumnName("PriceOverrideAmount");
                overridePrice.Property(p => p.Currency).HasColumnName("PriceOverrideCurrency");
            });
            variants.ToTable("ProductVariants");
        });

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();


        builder.Property(p => p.ThumbnailMediaId);


        builder.Navigation(p => p.MediaIds)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(p => p.MediaIds)
            .HasColumnName("MediaIds")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<List<Guid>>(v, JsonSerializerOptions.Default)!);
    }
}