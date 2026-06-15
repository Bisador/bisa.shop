using System.Net.Http.Json;
using Shared.Common;
using Shared.Media.Contracts;
using Shared.Media.Contracts.Requests;
using Shared.Media.Contracts.Responses;

namespace Shared.Media;

public sealed class MediaServiceClient(HttpClient httpClient) : IMediaServiceClient
{
    public async Task<Result> ValidateAsync(
        Guid tenantId,
        IEnumerable<Guid> ids,
        CancellationToken ct)
    {
        var enumerable = ids.ToList();
        var request = new ValidateMediaRequest(enumerable.ToList());

        var response =
            await httpClient.PostAsJsonAsync(
                "/api/media/validate",
                request,
                ct);

        if (!response.IsSuccessStatusCode)
            return Result.Failure(new InvalidMediaError());

        var payload = await response.Content.ReadFromJsonAsync<ValidateMediaResponse>(ct);

        if (payload is null)
            return Result.Failure(new InvalidMediaError());

        if (payload.ValidMediaIds.Count != enumerable.Count)
            return Result.Failure(new InvalidMediaError());

        return Result.Success();
    }

    public async Task<Result<IReadOnlyCollection<MediaMetadataResponse>>> GetMetadataBatchAsync(
        string category,
        IEnumerable<Guid> mediaIds,
        CancellationToken ct)
    {
        var ids = mediaIds.ToList();

        if (ids.Count == 0)
            return Result.Success<IReadOnlyCollection<MediaMetadataResponse>>([]);

        var query = string.Join("&", ids.Select(x => $"Ids={Uri.EscapeDataString(x.ToString())}"));
        var requestUrl = $"/api/media/metadata?{query}";

        var response = await httpClient.GetAsync(requestUrl, ct);

        if (!response.IsSuccessStatusCode)
            return Result.Failure<IReadOnlyCollection<MediaMetadataResponse>>(new InvalidMediaError());
 
        var metadata = await response.Content.ReadFromJsonAsync<List<MediaMetadataResponse>>(ct);

        if (metadata is null)
            return Result.Failure<IReadOnlyCollection<MediaMetadataResponse>>(new InvalidMediaError());

        return Result.Success<IReadOnlyCollection<MediaMetadataResponse>>(metadata);
    }

    public async Task<Result> LinkAsync(
        Guid tenantId,
        Guid mediaId,
        OwnerReference owner,
        CancellationToken cancellationToken)
    {
        var request =
            new LinkMediaRequest(
                owner.OwnerType,
                owner.OwnerId);

        var response =
            await httpClient.PostAsJsonAsync(
                $"/api/media/{mediaId}/links",
                request,
                cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(
                new MediaLinkFailedError());
    }

    public async Task<Result> UnlinkAsync(
        Guid tenantId,
        Guid mediaId,
        OwnerReference owner,
        CancellationToken cancellationToken)
    {
        var request =
            new UnlinkMediaRequest(
                owner.OwnerType,
                owner.OwnerId);

        using var message = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/media/{mediaId}/links");
        message.Content = JsonContent.Create(request);

        var response =
            await httpClient.SendAsync(
                message,
                cancellationToken);

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(
                new MediaLinkFailedError());
    }
}