namespace Minitwit.Core.Entities;

public record CreateTwit(Guid AuthorId, string Text)
{
    public readonly Guid AuthorId = AuthorId;
    public readonly string Text = Text;
}

