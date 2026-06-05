using CatalogService.Domain.Products.DomainEvents;
using CatalogService.Domain.Products.Exceptions;
using CatalogService.Domain.Products.Rules;

namespace CatalogService.Domain.Products.Aggregates;

public sealed class Product : AggregateRoot
{
    #region Descriptive

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Category { get; private set; } = null!;

    private readonly List<Guid> _mediaIds = [];
    public IReadOnlyCollection<Guid> MediaIds => _mediaIds.AsReadOnly();

    public Guid? ThumbnailMediaId { get; private set; }

    public void SetThumbnail(Guid mediaId)
    {
        ThumbnailMediaId = mediaId;
    }

    public void AddMedia(Guid mediaId)
    {
        if (_mediaIds.Contains(mediaId))
            return;

        _mediaIds.Add(mediaId);
    }

    public void RemoveMedia(Guid mediaId)
    {
        _mediaIds.Remove(mediaId);

        if (ThumbnailMediaId == mediaId)
            ThumbnailMediaId = null;
    }
    
    #endregion

    #region Commercial

    public Money Price { get; private set; } = null!;
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public bool IsDiscontinued { get; private set; }

    public void Publish()
    {
        if (string.IsNullOrWhiteSpace(Name) || Price.Amount <= 0)
            throw new CannotPublishWithoutNameAndPrice();

        CheckRule(new DiscontinuedProductsCannotBePublishedException(IsDiscontinued)); 

        IsPublished = true;
        PublishedAt = DateTime.UtcNow; 

        AddDomainEvent(new ProductPublished(Id));
    }

    public void ChangePrice(Money newPrice)
    {
        if (newPrice.Equals(Price))
            return;

        Price = newPrice; 

        AddDomainEvent(new ProductPriceChanged(Id, newPrice));
    }

    public void Discontinue()
    {
        if (IsDiscontinued)
            return;

        IsDiscontinued = true;
        IsPublished = false;

        AddDomainEvent(new ProductDiscontinued(Id));
    }

    #endregion

    #region Configurable

    #region Variants

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    public void AddVariant(ProductVariant variant)
    {
        if (_variants.Any(v => v.Sku == variant.Sku))
            throw new DuplicateVariantException();

        _variants.Add(variant);
        AddDomainEvent(new ProductVariantAdded(Id, variant.Sku));
    }

    public void RemoveVariant(string sku)
    {
        var variant = _variants.FirstOrDefault(v => v.Sku == sku);
        if (variant == null)
            throw new VariantNotFoundException(sku);

        _variants.Remove(variant);

        AddDomainEvent(new ProductVariantRemoved(Id, sku));
    }

    public Money GetPriceForSku(string sku)
    {
        var variant = _variants.FirstOrDefault(v => v.Sku == sku);
        if (variant == null)
            throw new VariantNotFoundException(sku);

        return variant.PriceOverride ?? Price;
    }

    public ProductVariant? GetVariantBySku(string sku) =>
        _variants.FirstOrDefault(v => v.Sku == sku);

    #endregion

    #endregion
   
    private Product()
    {
    }

    private Product(string name, string description, Money price, string category) : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category; 
        IsPublished = false;
        IsDiscontinued = false;

        AddDomainEvent(new ProductCreated(Id, Name, Category));
    }

    public static Product Create(string name, string description, Money price, string category)
    {  
        ArgumentException.ThrowIfNullOrEmpty(category, nameof(category)); 
        return new Product(name, description, price, category);
    }
 
}