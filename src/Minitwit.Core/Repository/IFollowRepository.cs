using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IFollowRepository
{
    public Task<Follow> CreateFollow(Guid followingAuthorId, Guid followedAuthorId);
    public Task DeleteFollow(Follow follow);
    public bool IsFollowing(Guid followingUserId, Guid followedAuthorId);
}
