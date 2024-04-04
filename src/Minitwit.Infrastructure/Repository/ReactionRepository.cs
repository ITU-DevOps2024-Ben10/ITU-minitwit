using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class ReactionRepository(MinitwitDbContext dbContext) : BaseRepository(dbContext), IReactionRepository
{
    public async Task AddReaction(ReactionType reaction, Guid cheepId, Guid authorId)
    {
        if (cheepId != Guid.Empty && authorId != Guid.Empty)
        {
            Reaction entity = new Reaction()
            {
                CheepId = cheepId,
                AuthorId = authorId,
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

    public async Task<ICollection<Reaction>> GetReactionsFromCheepIdAsync(Guid id)
    {
        return await db.Reactions.Where(r => r.CheepId == id).ToListAsync();
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
               count = await db.Reactions.CountAsync(r => r.CheepId == cheepId && r.ReactionType == reactionType);
           }
           else
           {
               throw new Exception("Cheep with id " + cheepId + " not found");
           }
           return count;
    }
    public async Task<bool> HasUserReactedAsync(Guid cheepId, Guid authorId)
    {
        //check if the user has reacted to the cheep
        return await db.Reactions.AnyAsync(r => r.CheepId == cheepId && r.AuthorId == authorId);
        
    }
}

