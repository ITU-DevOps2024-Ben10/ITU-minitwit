using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IAuthorRepository
{
    public void AddAuthorAsync(Author authorDto);


    public Task<ICollection<Author>> GetAllAuthorsAsync();
    public Task<ICollection<Author>> GetAuthorsByIdAsync(IEnumerable<Guid> authors);
    public Author GetAuthorById(Guid authorId);
    public Task<Author?> GetAuthorByIdAsync(Guid authorId);
    public Task<Author> GetAuthorByNameAsync(string name);
    public Author GetAuthorByEmail(string email);

    public ICollection<Cheep> GetCheepsByAuthor(Guid id);
    public ICollection<Cheep> GetCheepsByAuthor(Guid authorId, int page);
    public ICollection<Cheep> GetCheepsByAuthorAndFollowing(Guid authorId, int page);
    
    public int GetCheepCountByAuthor(Guid authorId);
    public int GetCheepCountByAuthorAndFollowing(Guid authorId);
    
    
    public int GetPageCountByAuthor(Guid authorId);
    public int GetPageCountByAuthorAndFollowing(Guid authorId);


    public Task<ICollection<Author>> GetFollowersByIdAsync(Guid authorId);
    public ICollection<Author> GetFollowingById(Guid authorId);
    

    
    public Task AddFollowAsync(Guid followingAuthorId, Guid followedAuthorId);
    public Task RemoveFollowAsync(Guid followingAuthorId, Guid followedAuthorId);
    
    
    
    public Task DeleteCheepsByAuthorId(Guid authorId);
    
    public Task RemoveAllFollowersByAuthorId(Guid id);
        
    public Task RemoveUserById(Guid id);

    public Task RemoveReactionsByAuthorId(Guid id);
    public Task SaveContextAsync();
}