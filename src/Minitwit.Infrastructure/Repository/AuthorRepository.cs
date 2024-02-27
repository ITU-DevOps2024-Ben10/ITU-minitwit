using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class AuthorRepository : BaseRepository, IAuthorRepository
{
    private readonly IFollowRepository _followRepository;
    public AuthorRepository(MinitwitDbContext minitwitDbContext) : base(minitwitDbContext)
    {
        db.Users.Include(e => e.Followers);
        db.Users.Include(e => e.Following);
        _followRepository = new FollowRepository(minitwitDbContext);
    }

    // ----- Add Author Methods ----- //
    public void AddAuthor(Author author)
    {
        db.Users.Add(author);
        db.SaveChanges();
    }


    // ----- Get Author Methods ----- //
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
        cheeps.Concat(GetCheepsByAuthor(id)).ToList();
        
        foreach (Author author in GetFollowingById(id))
        {
            cheeps.Add(GetCheepsByAuthor(author.Id));
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
        GetCheepsByAuthorAndFollowing(authorId).Count;

        return amountOfCheeps;
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
    public ICollection<Author?> GetFollowersById(Guid id)
    {
        Author author = db.Users.Include(a => a.Followers).ThenInclude(f => f.FollowingAuthor).SingleOrDefault(a => a.Id == id);
        
        ICollection<Author?> followers = new List<Author?>();
        
        foreach (var follow in author.Followers)
        {
            followers.Add(follow.FollowingAuthor);
        }

        return followers;
    }
    public ICollection<Author> GetFollowingById(Guid id)
    {
        // Initialize a collection to store the authors followed by the specified author

        // Query to retrieve the IDs of authors followed by the specified author
        var followingAuthorIds = db.Follows
            .Where(f => f.FollowedAuthorId == id)
            .Select(f => f.FollowingAuthorId)
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
        Follow follow = _followRepository.CreateFollow(followingAuthor, followedAuthor);
        
        followingAuthor.Following.Add(follow);
        followedAuthor.Followers.Add(follow);
        
        db.Users.Update(followingAuthor);
        db.Users.Update(followedAuthor);
        
        await db.SaveChangesAsync();
    }
    
    public async Task RemoveFollow(Author? followingAuthor, Author? followedAuthor) {
        // Load the Follow collections explicitly
        await db.Entry(followingAuthor).Collection(f => f.Followers).LoadAsync();
        await db.Entry(followedAuthor).Collection(u => u.Followers).LoadAsync();

        followingAuthor.Following.Remove(followingAuthor.Followers.FirstOrDefault(f => f.FollowedAuthorId == followedAuthor.Id)!);
        followedAuthor.Followers.Remove(followedAuthor.Followers.FirstOrDefault(f => f.FollowingAuthorId == followingAuthor.Id)!);

        db.Users.Update(followingAuthor);
        db.Users.Update(followedAuthor);
        
        await SaveContextAsync();
    }
    

    // ----- Delete Author Data Methods ----- //
    public async Task DeleteCheepsByAuthorId(Guid id)
    {
        Author? author = await GetAuthorByIdAsync(id);
        
        foreach (var cheep in author.Cheeps.ToList())
        {
            if (cheep.Reactions.Any())    
            {
                db.Reactions.RemoveRange(cheep.Reactions);
            }
            
            author.Cheeps.Remove(cheep);
        }

        db.SaveChanges();
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