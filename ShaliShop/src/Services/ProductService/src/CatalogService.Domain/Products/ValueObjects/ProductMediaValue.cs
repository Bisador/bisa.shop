namespace CatalogService.Domain.Products.ValueObjects;

public class ProductMediaValue(Guid mediaId) : ValueObject,IEquatable<ProductMediaValue>
{
    public Guid MediaId { get; init; } = mediaId;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MediaId;
    }

    public bool Equals(ProductMediaValue? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && MediaId.Equals(other.MediaId);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProductMediaValue)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), MediaId);
    }
}