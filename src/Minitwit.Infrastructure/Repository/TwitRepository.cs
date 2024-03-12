using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;


namespace Minitwit.Infrastructure.Repository;

public class TwitRepository : BaseRepository, ITwitRepository
{ 
    public TwitRepository(MinitwitDbContext DbContext) : base(DbContext)
    {
    }
    public ICollection<Twit> GetCheepsByPage(int page)
    {
        //Use EF to get the specified page of cheeps from the database
        ICollection<Twit> cheeps = db.Cheeps
            .OrderByDescending(c => c.TimeStamp)
            .Skip(PageSize * (page - 1))
            .Take(PageSize)
            .ToList();

        return cheeps;
    }

    public ICollection<Twit> GetCheepsByCount(int count)
    {
        //Use EF to get the specified count of cheeps from the database
        ICollection<Twit> cheeps = db.Cheeps
            .OrderByDescending(c => c.TimeStamp)
            .Take(count)
            .ToList();

        return cheeps;
    }
    
    public ICollection<Twit> GetCheepsFromAuthorByCount(Guid authorId, int count)
    {
        //Use EF to get the specified count of cheeps from an author from the database
        ICollection<Twit> cheeps = db.Cheeps
            .Where(c => c.AuthorId == authorId)
            .OrderByDescending(c => c.TimeStamp)
            .Take(count)
            .ToList();

        return cheeps;
    }

    public int GetCheepCount()
    {
        //Use EF to get the total number of cheeps from the database
        return db.Cheeps.Count();
    }

    public int GetPageCount()
    {
        return GetCheepCount()/PageSize+1;
    }

    public void DeleteCheepById(Guid cheepId)
    {
        //Delete the specified cheep from the database
        Twit? cheep = db.Cheeps.Find(cheepId);
        if (cheep != null)
        {
            db.Cheeps.Remove(cheep);
        }
        else
        {
            throw new Exception("Cheep with id " + cheepId + " not found");
        }

        db.SaveChanges();
    }

    public async Task AddCheep(Twit twit)
    {
        db.Cheeps.Add(twit);
        await db.SaveChangesAsync();
        Console.WriteLine("Cheep added async");
    }

    public async Task<Twit> AddCreateCheep(CreateTwit twit)
    {
        Twit entity = new Twit()
        {
            CheepId = new Guid(),
            Text = twit.Text,
            TimeStamp = DateTime.Now,
            AuthorId = twit.AuthorId
        };
        
        await AddCheep(entity);

        return entity;
    }

}