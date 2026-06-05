namespace Shared.Media.Contracts;

public sealed class MediaServiceOptions
{
    public const string SectionName = "MediaService";
    public string BaseUrl { get; init; } = "";
}