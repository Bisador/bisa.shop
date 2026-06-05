using OrderService.Domain.Products.DomainEvents;
using OrderService.Domain.Products.Rules;

namespace OrderService.Domain.Products.Aggregates;

public sealed class Product : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public Money Price { get; private set; } = null!;
    public int AvailableStock { get; private set; }
    public bool IsActive { get; private set; }
 
    private Product()
    {
    } // For EF

    private Product(string name, string description, Money price, int initialStock) : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        Price = price;
        AvailableStock = initialStock;
        IsActive = true;

        AddDomainEvent(new ProductAdded(Id, Name, Price));
    }

    public static Product Create(string name, string description, Money price, int initialStock)
    {
        CheckRule(new StockCannotBeNegative(initialStock));
        return new Product(name, description, price, initialStock);
    }

    public void UpdateDetails(string newName, string newDescription)
    {
        Name = newName;
        Description = newDescription;
        AddDomainEvent(new ProductUpdated(Id));
    }

    public void ChangePrice(Money newPrice)
    {
        Price = newPrice;
        AddDomainEvent(new ProductPriceChanged(Id, newPrice));
    }

    public void AdjustStock(int quantity)
    {
        AvailableStock += quantity;
        CheckRule(new StockCannotBeNegative(AvailableStock));
        if (AvailableStock == 0)
            AddDomainEvent(new ProductOutOfStock(Id));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new ProductDeactivated(Id));
    }
}