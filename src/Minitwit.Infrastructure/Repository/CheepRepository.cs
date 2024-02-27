using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;


namespace Minitwit.Infrastructure.Repository;

public class CheepRepository : BaseRepository, ICheepRepository
{ 
    public CheepRepository(MinitwitDbContext DbContext) : base(DbContext)
    {
    }
    public ICollection<Cheep> GetCheepsByPage(int page)
    {
        //Use EF to get the specified page of cheeps from the database
        ICollection<Cheep> cheeps = db.Cheeps
            .OrderByDescending(c => c.TimeStamp)
            .Skip(PageSize * (page - 1))
            .Take(PageSize)
            .ToList();

        return cheeps;
    }

    public ICollection<Cheep> GetCheepsByCount(int count)
    {
        //Use EF to get the specified page of cheeps from the database
        ICollection<Cheep> cheeps = db.Cheeps
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
        Cheep? cheep = db.Cheeps.Find(cheepId);
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

    public async Task AddCheep(Cheep cheep)
    {
      
        /*//Check if author is in database, if not add them too
        if (!db.Users.Any(a => a.Id == cheep.Author.Id)) db.Users.Add(cheep.Author); ### WHY?
        */
        

        db.Cheeps.Add(cheep);
        await db.SaveChangesAsync();
        Console.WriteLine("Cheep added async");
    }

    public async Task<Cheep> AddCreateCheep(CreateCheep cheep)
    {
        Cheep entity = new Cheep()
        {
            CheepId = new Guid(),
            Text = cheep.Text,
            TimeStamp = DateTime.Now,
            AuthorId = cheep.AuthorId
        };
        
        await AddCheep(entity);

        return entity;
    }

}