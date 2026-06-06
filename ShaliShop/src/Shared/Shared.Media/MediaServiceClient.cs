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
        IEnumerable<Guid> mediaIds,
        CancellationToken ct)
    {
        var enumerable = mediaIds.ToList();
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