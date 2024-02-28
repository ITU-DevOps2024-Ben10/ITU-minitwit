using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface IAuthorRepository
{
    public void AddAuthor(Author authorDto);


    public ICollection<Author> GetAuthors();
    public Author GetAuthorById(Guid authorId);
    public Task<Author?> GetAuthorByIdAsync(Guid authorId);
    public Author GetAuthorByName(string name);
    public Author GetAuthorByEmail(string email);

    public ICollection<Cheep> GetCheepsByAuthor(Guid id);
    public ICollection<Cheep> GetCheepsByAuthor(Guid authorId, int page);
    public ICollection<Cheep> GetCheepsByAuthorAndFollowing(Guid authorId, int page);
    
    public int GetCheepCountByAuthor(Guid authorId);
    public int GetCheepCountByAuthorAndFollowing(Guid authorId);
    
    
    public int GetPageCountByAuthor(Guid authorId);
    public int GetPageCountByAuthorAndFollowing(Guid authorId);


    public ICollection<Author> GetFollowersById(Guid authorId);
    public ICollection<Author> GetFollowingById(Guid authorId);
    

    
    public Task AddFollow(Author? followingAuthor, Author? followedAuthor);
    public Task RemoveFollow(Author? followingAuthor, Author? followedAuthor);
    
    
    
    public Task DeleteCheepsByAuthorId(Guid authorId);
    
    public Task RemoveAllFollowersByAuthorId(Guid id);
        
    public Task RemoveUserById(Guid id);

    public Task RemoveReactionsByAuthorId(Guid id);
    public Task SaveContextAsync();
}