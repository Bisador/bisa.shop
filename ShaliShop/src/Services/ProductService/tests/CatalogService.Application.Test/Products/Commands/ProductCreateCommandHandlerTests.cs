// using CatalogService.Application.Products.Commands.Create;
//
// namespace CatalogService.Application.Test.Products.Commands;
//
// public class ProductCreateCommandHandlerTests
// {
//     private readonly Mock<IProductRepository> _products = new();
//     private readonly Mock<ICatalogUnitOfWork> _unitOfWork = new();
//     private readonly ProductCreateCommandHandler _handler;
//
//     public ProductCreateCommandHandlerTests()
//     {
//         _handler = new ProductCreateCommandHandler(_products.Object, _unitOfWork.Object);
//     }
//     [Fact]
//     public async Task Should_create_product_and_return_id()
//     {
//         var command = new ProductCreateCommand(
//             "Tablet",
//             "Lightweight Android tablet",
//             new Money(129.99m),
//             "Electronics"
//         );
//   
//         _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
//             .Returns(Task.CompletedTask);
//
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().NotBeEmpty();
//  
//         _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//     }
//     [Fact]
//     public async Task Should_raise_ProductCreated_event()
//     {
//         var command = new ProductCreateCommand("Camera", "DSLR 24MP", new Money(499.00m), "Photography");
//
//         Product? captured = null;
//        
//         await _handler.Handle(command, CancellationToken.None);
//
//         captured.Should().NotBeNull();
//         captured!.DomainEvents.Any(e =>
//             e is ProductCreated created &&
//             created.AggregateId == captured.Id &&
//             created is {Name: "Camera", Category: "Photography"}
//         ).Should().BeTrue();
//     }
//
// }