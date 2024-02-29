namespace Minitwit.Core.Entities;
/// <summary>
/// This class represents the reaction a User can give to a cheep.
/// The reactions can be either a Like, Dislike, or Love.
/// Defined in ReactionType.cs.
/// </summary>
public class Reaction {
    public Guid CheepId { get; set; }

    public Guid AuthorId { get; set; }
    
    public ReactionType ReactionType { get; set; }
}