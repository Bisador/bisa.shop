namespace Shared.Media.Contracts.Responses;


public sealed record ValidateMediaResponse(IReadOnlyCollection<Guid> ValidMediaIds);