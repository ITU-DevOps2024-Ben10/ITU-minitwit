namespace Minitwit.Core.Entities;

public record CreateCheep(Author Author, string Text)
{
    public readonly Author Author = Author;
    public readonly string Text = Text;
}

