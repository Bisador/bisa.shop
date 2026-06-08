namespace CatalogService.Api.Requests;

public record ProductAddVariantRequest(string Sku, Dictionary<string, string> Options, decimal PriceOverride);