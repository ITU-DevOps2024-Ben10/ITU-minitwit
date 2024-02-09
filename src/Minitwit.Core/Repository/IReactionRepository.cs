using Chirp.Core.Entities;

namespace Chirp.Core.Repository;

public interface IReactionRepository
{
    public  Task AddReaction(ReactionType reaction, Guid cheepId, Guid authorId);
    public Task RemoveReaction(ReactionType reaction, Guid cheepId, Guid authorId);
    
    public Task<int> GetReactionCount(Guid cheepId, ReactionType reactionType);
    public Task<bool> HasUserReacted(Guid cheepId, Guid authorId);
}