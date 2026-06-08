using CatalogService.Domain.Products.ValueObjects;
using Shared.Persistence.Converters;

namespace CatalogService.Persistence.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Category).HasMaxLength(200);
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


        builder.Property(p => p.ThumbnailMedia);


        builder.Navigation(p => p.MediaItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(
            p => p.MediaItems,
            media =>
            {
                media.ToTable("ProductMedia");

                media.WithOwner()
                    .HasForeignKey("ProductId");

                media.Property(x => x.MediaId);

                media.HasKey(
                    "ProductId",
                    nameof(ProductMediaValue.MediaId));
            });
          
        builder.OwnsOne(p => p.ThumbnailMedia,
            thumbnail => { thumbnail.Property(p => p.MediaId).HasColumnName("ThumbnailId"); });
    }
}