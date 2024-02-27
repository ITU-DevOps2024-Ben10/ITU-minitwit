using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IFollowRepository
{
    public Task<Follow> CreateFollow(Author? followingAuthor, Author? followedAuthor);
    public Task DeleteFollow(Follow follow);
    public bool IsFollowing(Guid followingUserId, Guid followedAuthorId);
}
