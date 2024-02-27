using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class FollowRepository : BaseRepository, IFollowRepository
{
    public FollowRepository(MinitwitDbContext minitwitDbContext) : base(minitwitDbContext)
    {
    }
    public async Task<Follow> CreateFollow(Author? followingAuthor, Author? followedAuthor)
    {
        Follow follow = new()
        {
            FollowingAuthorId = followingAuthor!.Id,
            FollowedAuthorId = followedAuthor!.Id
        };
        db.Follows.Add(follow);
        await db.SaveChangesAsync();
        return follow;
    }
    
    public async Task DeleteFollow(Follow follow)
    {
        db.Follows.Remove(follow);
        await db.SaveChangesAsync();
    }

    public bool IsFollowing(Guid followingUserId, Guid followedUserId)
    {
        if (followingUserId == Guid.Empty || followedUserId == Guid.Empty) 
            return false;

        // Check if there exists a Follow record where the following user ID matches followingUserId
        // and the followed user ID matches followedUserId
        var isFollowing = db.Follows
            .Any(f => f.FollowingAuthorId == followingUserId && f.FollowedAuthorId == followedUserId);

        return isFollowing;
    }

    public bool HasFollowers(Guid authorId)
    {
        return db.Follows.Any(e => e.FollowedAuthorId == authorId);
    }
}