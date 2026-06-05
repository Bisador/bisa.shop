using Shared.Common;

namespace Shared.Media.Contracts;

public interface IMediaServiceClient
{
    Task<Result> ValidateAsync(
        Guid tenantId,
        IEnumerable<Guid> mediaIds,
        CancellationToken ct);

    Task<Result> LinkAsync(
        Guid tenantId,
        Guid mediaId,
        OwnerReference owner,
        CancellationToken cancellationToken);

    Task<Result> UnlinkAsync(
        Guid tenantId,
        Guid mediaId,
        OwnerReference owner,
        CancellationToken cancellationToken);
}