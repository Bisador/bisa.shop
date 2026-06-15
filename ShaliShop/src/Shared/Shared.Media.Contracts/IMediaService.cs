using Shared.Common;
using Shared.Media.Contracts.Responses;

namespace Shared.Media.Contracts;

public interface IMediaService
{
    Task<Result> ValidateAsync(
        string category,
        IEnumerable<Guid> ids, 
        CancellationToken ct); 
}