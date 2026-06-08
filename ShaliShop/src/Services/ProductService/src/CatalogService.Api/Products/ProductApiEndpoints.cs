using CatalogService.Api.Requests;
using CatalogService.Application.Abstraction.Products;
using CatalogService.Application.Products.Commands.AddVariant;
using CatalogService.Application.Products.Commands.ChangePrice;
using CatalogService.Application.Products.Commands.Create;
using CatalogService.Application.Products.Commands.Discontinue;
using CatalogService.Application.Products.Commands.Publish;
using CatalogService.Application.Products.Commands.RemoveVariant;
using CatalogService.Application.Products.Queries.GetAll;
using CatalogService.Application.Products.Queries.GetById;
using Microsoft.AspNetCore.Routing;

namespace CatalogService.Api.Products;

public static class ProductApiEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var catalogGroup = endpoints.MapGroup("/api/catalog/products");
        catalogGroup.MapGet("/", GetAllProducts);
        catalogGroup.MapGet("/{id:guid}", GetProductById);
        catalogGroup.MapPost("/", CreateProduct);
        catalogGroup.MapPost("/{id:guid}/publish", PublishProduct);
        catalogGroup.MapPost("/{id:guid}/discontinue", DiscontinueProduct);
        catalogGroup.MapPost("/{id:guid}/variants", AddVariant);
        catalogGroup.MapDelete("/{id:guid}/variants/{sku}", RemoveVariant);
        catalogGroup.MapPut("/{id:guid}/price", ChangePrice);

        return endpoints;
    }

    private static async Task<Results<Ok<PaginatedResult<ProductDto>>, BadRequest>> GetAllProducts(
        [FromServices] IMediator mediator,
        [AsParameters] ProductGetAllQuery query)
    {
        var result = await mediator.Send(query);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<ProductDto>, NotFound, BadRequest>> GetProductById(
        Guid id,
        [FromServices] IMediator mediator)
    {
        if (id == Guid.Empty)
            return TypedResults.BadRequest();
        var result = await mediator.Send(new ProductGetByIdQuery(id));

        return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    private static async Task<Results<Created<Guid>, ProblemHttpResult>> CreateProduct(
        ProductCreateRequest request,
        IMediator mediator)
    {
        var command = new ProductCreateCommand(
            Name: request.Name,
            Description: request.Description,
            Amount: request.Amount,
            Currency: request.Currency,
            Category: request.Category,
            ThumbnailMediaId: request.ThumbnailMediaId,
            MediaIds: request.MediaIds);
        var result = await mediator.Send(command);

        return result.IsSuccess
            ? TypedResults.Created($"/api/catalog/products/{result.Value}", result.Value)
            : result.Problem();
    }


    private static async Task<Results<NoContent, ProblemHttpResult>> PublishProduct(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new ProductPublishCommand(id));
        return result.IsSuccess ? TypedResults.NoContent() : result.Problem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DiscontinueProduct(
        Guid id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new ProductDiscontinueCommand(id));
        return result.IsSuccess ? TypedResults.NoContent() : result.Problem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> AddVariant(
        Guid id,
        [FromBody] ProductAddVariantRequest request, // assume DTO → command mapping
        [FromServices] IMediator mediator)
    {
        var command = new ProductAddVariantCommand(id, request.Sku, request.Options, Money.From(request.PriceOverride));
        var result = await mediator.Send(command);
        return result.IsSuccess ? TypedResults.NoContent() : result.Problem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RemoveVariant(
        Guid id,
        string sku,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new ProductRemoveVariantCommand(id, sku));
        return result.IsSuccess ? TypedResults.NoContent() : result.Problem();
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> ChangePrice(
        Guid id,
        [FromBody] ChangePriceRequest request,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new ProductChangePriceCommand(id, Money.From(request.NewPrice)));
        return result.IsSuccess ? TypedResults.NoContent() : result.Problem();
    }
}