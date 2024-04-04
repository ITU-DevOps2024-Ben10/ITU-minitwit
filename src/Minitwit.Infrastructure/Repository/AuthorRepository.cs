using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class AuthorRepository : BaseRepository, IAuthorRepository
{
    private readonly IFollowRepository _followRepository;
    public AuthorRepository(MinitwitDbContext minitwitDbContext, IFollowRepository followRepository) : base(minitwitDbContext)
    {
        _followRepository = followRepository;
    }

    // ----- Add Author Methods ----- //
    public async void AddAuthorAsync(Author author)
    {
        await db.Users.AddAsync(author);
        await db.SaveChangesAsync();
    }


    // ----- Get Author Methods ----- //

    public async Task<ICollection<Author>> GetAllAuthorsAsync()
    {
        return await db.Users.ToListAsync();
    }

    public async Task<ICollection<Author>> GetAuthorsByIdAsync(IEnumerable<Guid> authorIds)
    {
        // Assuming 'db' is your DbContext instance.
        return await db.Users
            .Where(a => authorIds.Contains(a.Id))
            .ToListAsync();
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
    
    public async Task<Author> GetAuthorByNameAsync(string name)
    {
        Author? author = await db.Users.FirstOrDefaultAsync(a => a.UserName == name)!;
            
        return author!;
    }
    
    public async Task<Author> GetAuthorByEmail(string email)
    {
        Author? author = await db.Users.FirstOrDefaultAsync(a => a.Email == email)!;
            
        return author!;
    }


    // ----- Get Cheeps By Author and Page Methods ----- //
    public async Task<ICollection<Cheep>> GetCheepsByAuthorAsync(Guid id)
    {
        return await db.Cheeps
            .Where(e => e.AuthorId == id)
            .ToListAsync();
    }
    
    public async Task<ICollection<Cheep>> GetCheepsByAuthor(Guid id, int page)
    {
        var cheeps = await GetCheepsByAuthorAsync(id);
        
        //Check that author has cheeps
        if (cheeps == null || cheeps.Count == 0)
        {
            throw new Exception("This author has no cheeps");
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

    public async Task<ICollection<Cheep>> GetCheepsByAuthorAndFollowingAsync(Guid id)
    {
        ICollection<Cheep> cheeps = new List<Cheep>(await GetCheepsByAuthorAsync(id));
        
        foreach (Author author in GetFollowingById(id))
        {
            cheeps = cheeps.Concat(await GetCheepsByAuthorAsync(author.Id)).ToList();
        }

        return cheeps;
    }
    
    public async Task<ICollection<Cheep>> GetCheepsByAuthorAndFollowing(Guid id, int page)
    {
        Author author = GetAuthorById(id);
        //Get cheeps from the author, and append cheeps from followers to that list
        ICollection<Author> following = GetFollowingById(id);
        ICollection<Cheep> cheeps = new List<Cheep>();

        // Add all the users cheeps to the list without pagination
        foreach (var cheepDto in await GetCheepsByAuthorAsync(id))
        {
            cheeps.Add(cheepDto);
        }

        foreach (Author? follower in following)
        {
            ICollection<Cheep> followingCheeps = await GetCheepsByAuthorAsync(follower.Id);
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
    public async Task<int> GetCheepCountByAuthorAsync(Guid authorId)
    {
        ICollection<Cheep> cheeps = await GetCheepsByAuthorAsync(authorId);
        //Check that author has cheeps
        if (cheeps.Count == 0 || cheeps == null)
        {
            return 0;
        }

        return cheeps.Count;
    }
    
    public async Task<int> GetCheepCountByAuthorAndFollowing(Guid authorId)
    {
        ICollection<Cheep> cheeps = await GetCheepsByAuthorAndFollowingAsync(authorId);
        return cheeps.Count;
    }


    // ----- Get Page Count Methods ----- //
    public async Task<int> GetPageCountByAuthor(Guid authorId)
    {
        return await GetCheepCountByAuthorAsync(authorId) / PageSize + 1;
    }
    
    public async Task<int> GetPageCountByAuthorAndFollowing(Guid authorId)
    {
        return await GetCheepCountByAuthorAndFollowing(authorId) / PageSize + 1;
    }
    // ----- Get Followers and Following Methods ----- //
    public async Task<ICollection<Author>> GetFollowersByIdAsync(Guid id)
    {
        // Query to retrieve the IDs of authors followed by the specified author
        List<Guid> followedAuthorIds = await db.Follows
            .Where(f => f.FollowedAuthorId == id)
            .Select(f => f.FollowingAuthorId)
            .ToListAsync();

        // Query to retrieve the author entities based on the followed author IDs
        ICollection<Author> followedAuthors = await db.Users
            .Where(a => followedAuthorIds.Contains(a.Id))
            .ToListAsync();

        return followedAuthors;
    }
    
    public ICollection<Author> GetFollowingById(Guid id)
    {
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

    public async Task<bool> AuthorExists(Guid id)
    {
        Author author = await GetAuthorByIdAsync(id);
        return author.Id != Guid.Empty;
    }


    // ----- Add/Remove Follow Methods ----- //
    public async Task AddFollowAsync(Guid followingAuthorId, Guid followedAuthorId)
    {
        await _followRepository.CreateFollowAsync(followingAuthorId, followedAuthorId);
    }

    public async Task RemoveFollowAsync(Guid followingAuthorId, Guid followedAuthorId)
    {
        Follow follow = await db.Follows
            .FirstOrDefaultAsync(e => e.FollowedAuthorId == followedAuthorId && e.FollowingAuthorId == followingAuthorId)!;
        await _followRepository.DeleteFollowAsync(follow);
    }

    public async Task RemoveFollow(Author? followingAuthor, Author? followedAuthor)
    {
        Follow follow = await db.Follows
            .FirstOrDefaultAsync(e => e.FollowedAuthorId == followedAuthor!.Id && e.FollowingAuthorId == followingAuthor!.Id)!;
        await _followRepository.DeleteFollowAsync(follow);
    }
    

    // ----- Delete Author Data Methods ----- //
    public async Task DeleteCheepsByAuthorIdAsync(Guid id)
    {
        var cheeps = await GetCheepsByAuthorAsync(id);

        foreach (var cheep in cheeps)
        {
            // Find reactions associated with the current cheep and delete them
            List<Reaction> reactions = await db.Reactions
                .Where(r => r.CheepId == cheep.CheepId)
                .ToListAsync();

            db.Reactions.RemoveRange(reactions);

            // Delete the cheep itself
            db.Cheeps.Remove(cheep);
        }

        await db.SaveChangesAsync();
    }

    public async Task RemoveAllFollowersByAuthorIdAsync(Guid id)
    {
        Author? user = await GetAuthorByIdAsync(id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var follows = await db.Follows.Where(f => f.FollowedAuthorId == id || f.FollowingAuthorId == id).ToListAsync();
        db.Follows.RemoveRange(follows);
    }
    
    public async Task RemoveUserByIdAsync(Guid id)
    {
        Author? user = await GetAuthorByIdAsync(id);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    public async Task RemoveReactionsByAuthorIdAsync(Guid id)
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