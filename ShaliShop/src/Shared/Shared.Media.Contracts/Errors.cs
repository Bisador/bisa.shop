using Shared.Common;

namespace Shared.Media.Contracts;

public record InvalidMediaError() : Error(nameof(InvalidMediaError), "Invalid Media Error");

public record MediaLinkFailedError() : Error(nameof(MediaLinkFailedError), "Invalid Media Link Error");