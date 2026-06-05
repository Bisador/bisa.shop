using CatalogService.Api.Products;
using Microsoft.AspNetCore.Routing;

namespace CatalogService.Api;

public static class CatalogApiEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapProductEndpoints(); 
        return endpoints;
    }
}