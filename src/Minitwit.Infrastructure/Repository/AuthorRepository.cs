using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class AuthorRepository : BaseRepository, IAuthorRepository
{
    private readonly IFollowRepository _followRepository;
    public AuthorRepository(MinitwitDbContext minitwitDbContext) : base(minitwitDbContext)
    {
        _followRepository = new FollowRepository(minitwitDbContext);
    }

    // ----- Add Author Methods ----- //
    public void AddAuthor(Author author)
    {
        db.Users.Add(author);
        db.SaveChanges();
    }


    // ----- Get Author Methods ----- //

    public ICollection<Author> GetAuthors()
    {
        return db.Users.ToList();
    }
    
    public Author GetAuthorById(Guid authorId)
    {
        Author author = db.Users.FirstOrDefault(a => a.Id == authorId)!;
            
        return author;
    }
    
    public async Task<Author?> GetAuthorByIdAsync(Guid authorId)
    {
        Author? author = await db.Users.FirstOrDefaultAsync(a => a.Id == authorId);
        return author!;
    }
    
    public Author GetAuthorByName(string name)
    {
        Author author = db.Users.FirstOrDefault(a => a.UserName == name)!;
            
        return author;
    }
    
    public Author GetAuthorByEmail(string email)
    {
        Author author = db.Users.FirstOrDefault(a => a.Email == email)!;
            
        return author;
    }


    // ----- Get Cheeps By Author and Page Methods ----- //


    public ICollection<Cheep> GetCheepsByAuthor(Guid id)
    {
        Author author = GetAuthorById(id);
        return db.Cheeps
            .Where(e => e.AuthorId == author.Id)
            .ToList();
    }
    
    public ICollection<Cheep> GetCheepsByAuthor(Guid id, int page)
    {

        Author author = GetAuthorById(id);
        var cheeps = GetCheepsByAuthor(id);
        
        //Check that author has cheeps
        if (cheeps == null || cheeps.Count == 0)
        {
            throw new Exception("Author " + author.UserName + " has no cheeps");
        }

        if(page < 1){
            page = 1;
        }

        int pageSizeIndex = (page - 1) * PageSize;

        if (cheeps.Count < pageSizeIndex + PageSize)
            return cheeps.ToList().GetRange(pageSizeIndex, cheeps.Count - pageSizeIndex)
                .OrderByDescending(c => c.TimeStamp).ToList();
        if(cheeps.Count > PageSize) return cheeps.ToList().GetRange(pageSizeIndex,PageSize).OrderByDescending(c => c.TimeStamp).ToList();
        return cheeps.OrderByDescending(c => c.TimeStamp).ToList();
    }

    public ICollection<Cheep> GetCheepsByAuthorAndFollowing(Guid id)
    {
        ICollection<Cheep> cheeps = new List<Cheep>();
        cheeps = cheeps.Concat(GetCheepsByAuthor(id)).ToList();
        
        foreach (Author author in GetFollowingById(id))
        {
            cheeps = cheeps.Concat(GetCheepsByAuthor(author.Id)).ToList();
        }

        return cheeps;
    }
    
    public ICollection<Cheep> GetCheepsByAuthorAndFollowing(Guid id, int page)
    {
        Author author = GetAuthorById(id);
        //Get cheeps from the author, and append cheeps from followers to that list
        ICollection<Author> following = GetFollowingById(id);
        ICollection<Cheep> cheeps = new List<Cheep>();

        // Add all the users cheeps to the list without pagination
        foreach (var cheepDto in GetCheepsByAuthor(id))
        {
            cheeps.Add(cheepDto);
        }

        foreach (Author? follower in following)
        {
            ICollection<Cheep> followingCheeps = GetCheepsByAuthor(follower.Id);
            //If follower has no cheeps, skip them
            if (followingCheeps.Count == 0)
            {
                continue;
            }

            //Add each cheep from the follower to the list
            //TODO Try to find alternative to foreach
            foreach (var cheepDto in followingCheeps)
            {
                cheeps.Add(cheepDto);
            }

        }
        //Sort the cheeps according to timestamp, latest first
        cheeps = cheeps.OrderByDescending(c => c.TimeStamp).ToList();

        int pageSizeIndex = (page - 1) * PageSize;

        if (cheeps.Count < pageSizeIndex + PageSize) return cheeps.ToList<Cheep>().GetRange(pageSizeIndex, cheeps.Count - pageSizeIndex);
        if (cheeps.Count > PageSize) return cheeps.ToList<Cheep>().GetRange(pageSizeIndex, PageSize);
        return cheeps;
    }

    
    // ----- Get Cheeps By Author Methods ----- //
    public int GetCheepCountByAuthor(Guid authorId)
    {
        ICollection<Cheep> cheeps = GetCheepsByAuthor(authorId);
        //Check that author has cheeps
        if (cheeps.Count == 0 || cheeps == null)
        {
            return 0;
        }

        return cheeps.Count;
    }
    
    public int GetCheepCountByAuthorAndFollowing(Guid authorId)
    {
        return GetCheepsByAuthorAndFollowing(authorId).Count;
    }


    // ----- Get Page Count Methods ----- //
    public int GetPageCountByAuthor(Guid authorId)
    {
        return GetCheepCountByAuthor(authorId) / PageSize + 1;
    }
    
    public int GetPageCountByAuthorAndFollowing(Guid authorId)
    {
        return GetCheepCountByAuthorAndFollowing(authorId) / PageSize + 1;
    }
    // ----- Get Followers and Following Methods ----- //
    public ICollection<Author> GetFollowersById(Guid id)
    {
        // Initialize a collection to store the authors followed by the specified author

        // Query to retrieve the IDs of authors followed by the specified author
        var followedAuthorIds = db.Follows
            .Where(f => f.FollowedAuthorId == id)
            .Select(f => f.FollowingAuthorId)
            .ToList();

        // Query to retrieve the author entities based on the followed author IDs
        ICollection<Author> followedAuthors = db.Users
            .Where(a => followedAuthorIds.Contains(a.Id))
            .ToList();

        return followedAuthors;
    }
    public ICollection<Author> GetFollowingById(Guid id)
    {
        // Initialize a collection to store the authors followed by the specified author

        // Query to retrieve the IDs of authors followed by the specified author
        var followingAuthorIds = db.Follows
            .Where(f => f.FollowingAuthorId == id)
            .Select(f => f.FollowedAuthorId)
            .ToList();

        // Query to retrieve the author entities based on the followed author IDs
        ICollection<Author> followingAuthors = db.Users
            .Where(a => followingAuthorIds.Contains(a.Id))
            .ToList();

        return followingAuthors;
    }



    // ----- Add/Remove Follow Methods ----- //
    public async Task AddFollow(Author? followingAuthor, Author? followedAuthor)
    {
        await _followRepository.CreateFollow(followingAuthor, followedAuthor);
        
    }
    
    public async Task RemoveFollow(Author? followingAuthor, Author? followedAuthor)
    {
        Follow follow = db.Follows
            .FirstOrDefault(e => e.FollowedAuthorId == followedAuthor!.Id && e.FollowingAuthorId == followingAuthor!.Id)!;
        await _followRepository.DeleteFollow(follow);
    }
    

    // ----- Delete Author Data Methods ----- //
    public async Task DeleteCheepsByAuthorId(Guid id)
    {
        var cheeps = GetCheepsByAuthor(id);

        foreach (var cheep in cheeps)
        {
            // Find reactions associated with the current cheep and delete them
            var reactions = db.Reactions
                .Where(r => r.CheepId == cheep.CheepId)
                .ToList();

            db.Reactions.RemoveRange(reactions);

            // Delete the cheep itself
            db.Cheeps.Remove(cheep);
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoveAllFollowersByAuthorId(Guid id)
    {
        Author? user = await GetAuthorByIdAsync(id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var follows = await db.Follows.Where(f => f.FollowedAuthorId == id || f.FollowingAuthorId == id).ToListAsync();
        db.Follows.RemoveRange(follows);
    }
    
    public async Task RemoveUserById(Guid id)
    {
        Author? user = await GetAuthorByIdAsync(id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    public async Task RemoveReactionsByAuthorId(Guid id)
    {
        Author? user = await GetAuthorByIdAsync(id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var reactions = await db.Reactions.Where(r => r.AuthorId == id).ToListAsync();
        if (reactions != null)
        {
            db.Reactions.RemoveRange(reactions);
        }
    }
    
    
    // ----- Save Context Method ----- //
    public async Task SaveContextAsync()
    {
        await db.SaveChangesAsync();
    }
}