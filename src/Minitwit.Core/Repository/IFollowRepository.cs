using Chirp.Core.Entities;

namespace Chirp.Core.Repository;

public interface IFollowRepository
{
    public Follow CreateFollow(Author? followingAuthor, Author? followedAuthor);
    public bool IsFollowing(Guid followingUserId, Guid followedAuthorId);
}
