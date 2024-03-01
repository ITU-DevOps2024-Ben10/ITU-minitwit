namespace Minitwit.Core.Entities;

public record CreateCheep(Guid AuthorId, string Text)
{
    public readonly Guid AuthorId = AuthorId;
    public readonly string Text = Text;
}

