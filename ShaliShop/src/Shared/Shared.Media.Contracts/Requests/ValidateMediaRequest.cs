namespace Shared.Media.Contracts.Requests;

public sealed record ValidateMediaRequest(IReadOnlyCollection<Guid> MediaIds);