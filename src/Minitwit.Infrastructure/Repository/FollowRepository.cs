using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class FollowRepository : BaseRepository, IFollowRepository
{
    public FollowRepository(MinitwitDbContext minitwitDbContext) : base(minitwitDbContext)
    {
    }
    
    public async Task<Follow> CreateFollowAsync(Guid followingAuthorId, Guid followedAuthorId)
    {
        Follow follow = new()
        {
            FollowingAuthorId = followingAuthorId,
            FollowedAuthorId = followedAuthorId
        };
        await db.Follows.AddAsync(follow);
        await db.SaveChangesAsync();
        return follow;
    }
    
    public async Task DeleteFollowAsync(Follow follow)
    {
        db.Follows.Remove(follow);
        await db.SaveChangesAsync();
    }

    public async Task<bool> IsFollowingAsync(Guid followingUserId, Guid followedUserId)
    {
        if (followingUserId == Guid.Empty || followedUserId == Guid.Empty) 
            return false;

        bool isFollowing = await db.Follows
            .AnyAsync(f => f.FollowingAuthorId == followingUserId && f.FollowedAuthorId == followedUserId);

        return isFollowing;
    }


    public async Task<bool> HasFollowersAsync(Guid authorId)
    {
        return await db.Follows.AnyAsync(e => e.FollowedAuthorId == authorId);
    }
}