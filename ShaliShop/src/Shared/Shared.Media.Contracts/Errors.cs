using Shared.Common;

namespace Shared.Media.Contracts;

public record InvalidMediaError() : Error(nameof(InvalidMediaError), "Invalid Media");
public record MediaIsNotExistedError() : Error(nameof(MediaIsNotExistedError), "Media Is Not Existed");
public record InvalidMediaCategoryError() : Error(nameof(InvalidMediaCategoryError), "Invalid Media Category");

public record MediaLinkFailedError() : Error(nameof(MediaLinkFailedError), "Invalid Media Link");