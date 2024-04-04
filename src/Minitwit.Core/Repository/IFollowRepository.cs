using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IFollowRepository
{
    public Task<Follow> CreateFollowAsync(Guid followingAuthorId, Guid followedAuthorId);
    public Task DeleteFollowAsync(Follow follow);
    public Task<bool> IsFollowingAsync(Guid followingUserId, Guid followedAuthorId);
}
