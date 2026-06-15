using Shared.Common;
using Shared.Media.Contracts.Responses;

namespace Shared.Media.Contracts;

public interface IMediaServiceClient
{
    Task<Result> ValidateAsync(
        Guid tenantId, 
        IEnumerable<Guid> ids,
        CancellationToken ct);
    
    Task<Result<IReadOnlyCollection<MediaMetadataResponse>>> GetMetadataBatchAsync( 
        string category,
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