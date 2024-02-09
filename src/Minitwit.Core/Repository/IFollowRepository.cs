using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IFollowRepository
{
    public Follow CreateFollow(Author? followingAuthor, Author? followedAuthor);
    public bool IsFollowing(Guid followingUserId, Guid followedAuthorId);
}
