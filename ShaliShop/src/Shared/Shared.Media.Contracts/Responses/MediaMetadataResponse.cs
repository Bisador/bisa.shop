namespace Shared.Media.Contracts.Responses;

public sealed record MediaMetadataResponse
{ 
    public bool Exists { get; init; }
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public MediaStatus Status { get; set; }
    public string? ContentType { get; set; }
    public MediaAccessLevel AccessLevel { get; set; }
    public MediaPurpose? MediaPurpose { get; init; }
    public long? Size { get; init; }
    public string? Category { get; init; }
}

public enum MediaAccessLevel
{
    Managed = 0,
    Public = 1
}

public enum MediaStatus
{
    Uploading = 1,
    Available = 2,
    Deleting = 3,
    Deleted = 4
}

public enum MediaPurpose
{
    PersonalStorage = 1,
    Attachment = 2
}