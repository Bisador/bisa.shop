namespace Shared.Media.Contracts.Requests;

public sealed record LinkMediaRequest(
    string OwnerType,
    string OwnerId);