using Chirp.Core.Entities;
using Chirp.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Minitwit.Infrastructure.Repository;

public class ReactionRepository(MinitwitDbContext dbContext) : BaseRepository(dbContext), IReactionRepository
{
    public async Task AddReaction(ReactionType reaction, Guid cheepId, Guid authorId)
    {
        Cheep? cheep = await db.Cheeps.FindAsync(cheepId);
        Author? author = await db.Users.FindAsync(authorId);
        if (cheep != null)
        {
            Reaction entity = new Reaction()
            {
                CheepId = cheepId,
                Cheep = cheep,
                AuthorId = authorId,
                Author = author,
                ReactionType = reaction
            };
            db.Reactions.Add(entity);
            await db.SaveChangesAsync();
        }
        else
        {
            //
            throw new Exception("Cheep with id " + cheepId + " not found");
        }
    }
    public async Task RemoveReaction(ReactionType reaction, Guid cheepId, Guid authorId)
    {
        Reaction? entity = await db.Reactions.FindAsync(cheepId, authorId);
        if (entity != null)
        {
            db.Reactions.Remove(entity); 
            await db.SaveChangesAsync();
        }       
    }
    
    public async Task<int> GetReactionCount(Guid cheepId, ReactionType reactionType)
    {
           Cheep? cheep = await db.Cheeps.FindAsync(cheepId);
           int count = 0; 
            if (cheep != null)
           { 
               count = await db.Reactions.CountAsync(r => r.Cheep == cheep && r.ReactionType == reactionType);
           }
           else
           {
               throw new Exception("Cheep with id " + cheepId + " not found");
           }
            return count;
    }
    public async Task<bool> HasUserReacted(Guid cheepId, Guid authorId)
    {
        //check if the user has reacted to the cheep
        bool hasReacted = await db.Reactions.AnyAsync(r => r.CheepId == cheepId && r.AuthorId == authorId);
      
        return hasReacted;
    }
}

