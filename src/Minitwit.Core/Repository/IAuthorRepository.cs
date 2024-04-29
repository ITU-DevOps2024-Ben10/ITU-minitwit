using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IAuthorRepository
{
    public void AddAuthorAsync(Author authorDto);

    public Task<ICollection<Author>> GetAllAuthorsAsync();
    public Task<ICollection<Author>> GetAuthorsByIdAsync(IEnumerable<Guid> authors);
    public Task<Author?> GetAuthorByIdAsync(Guid authorId);
    public Task<Author> GetAuthorByNameAsync(string name);
    public Task<Author> GetAuthorByEmail(string email);

    public Task<ICollection<Cheep>> GetCheepsByAuthorAsync(Guid id);
    public Task<ICollection<Cheep>> GetCheepsByAuthor(Guid authorId, int page);
    public Task<ICollection<Cheep>> GetCheepsByAuthorAndFollowing(Guid authorId, int page);

    public Task<int> GetCheepCountByAuthorAsync(Guid authorId);
    public Task<int> GetCheepCountByAuthorAndFollowing(Guid authorId);

    public Task<int> GetPageCountByAuthor(Guid authorId);
    public Task<int> GetPageCountByAuthorAndFollowing(Guid authorId);

    public Task<ICollection<Author>> GetFollowersByIdAsync(Guid authorId);
    public Task<ICollection<Author>> GetFollowingByIdAsync(Guid authorId);

    public Task AddFollowAsync(Guid followingAuthorId, Guid followedAuthorId);
    public Task RemoveFollowAsync(Guid followingAuthorId, Guid followedAuthorId);

    public Task DeleteCheepsByAuthorIdAsync(Guid authorId);

    public Task RemoveAllFollowersByAuthorIdAsync(Guid id);

    public Task RemoveUserByIdAsync(Guid id);

    public Task RemoveReactionsByAuthorIdAsync(Guid id);
    public Task SaveContextAsync();
}
