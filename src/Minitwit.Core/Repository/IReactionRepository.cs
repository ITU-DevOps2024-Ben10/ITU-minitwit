using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IReactionRepository
{
    public  Task AddReaction(ReactionType reaction, Guid cheepId, Guid authorId);
    public Task RemoveReaction(ReactionType reaction, Guid cheepId, Guid authorId);

    public Task<ICollection<Reaction>> GetReactionsFromCheepIdAsync(Guid id);
    public Task<int> GetReactionCount(Guid cheepId, ReactionType reactionType);
    public Task<bool> HasUserReacted(Guid cheepId, Guid authorId);
}