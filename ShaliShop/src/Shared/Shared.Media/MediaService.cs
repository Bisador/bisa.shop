using Shared.Common;
using Shared.Media.Contracts;

namespace Shared.Media;

public sealed class MediaService(IMediaServiceClient mediaClient) : IMediaService
{
    public static string PersonalCategory => "Personal";

    public async Task<Result> ValidateAsync(string category, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var mediaMetadata = await mediaClient.GetMetadataBatchAsync(
            category: category,
            mediaIds: ids,
            ct: ct);

        if (mediaMetadata.IsFailure)
            return Result.Failure<Guid>(mediaMetadata.Error!);

        var metadataItems = (mediaMetadata.Value ?? []).ToList();
        if (metadataItems.Any(p => !p.Exists))
            return Result.Failure<Guid>(new MediaIsNotExistedError());

        if (metadataItems.Any(p => p.Category != PersonalCategory && p.Category != category))
            return Result.Failure<Guid>(new InvalidMediaCategoryError());

        return Result.Success();
    }
}