using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.Requests;

public record ChangePriceRequest( 
    decimal NewPrice
);