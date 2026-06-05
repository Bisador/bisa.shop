namespace Shared.Media.Contracts.Requests;

public sealed record UnlinkMediaRequest(
    string OwnerType,
    string OwnerId);