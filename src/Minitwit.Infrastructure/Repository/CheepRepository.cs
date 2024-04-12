using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Infrastructure.Repository;

public class CheepRepository : BaseRepository, ICheepRepository
{
    public CheepRepository(MinitwitDbContext DbContext)
        : base(DbContext) { }

    public async Task<ICollection<Cheep>> GetCheepsByPageAsync(int page)
    {
        //Use EF to get the specified page of cheeps from the database
        ICollection<Cheep> cheeps = await db
            .Cheeps.OrderByDescending(c => c.TimeStamp)
            .Skip(PageSize * (page - 1))
            .Take(PageSize)
            .ToListAsync();

        return cheeps;
    }

    public async Task<ICollection<Cheep>> GetCheepsByCountAsync(int count)
    {
        //Use EF to get the specified count of cheeps from the database
        ICollection<Cheep> cheeps = await db
            .Cheeps.OrderByDescending(c => c.TimeStamp)
            .Take(count)
            .ToListAsync();

        return cheeps;
    }

    public async Task<ICollection<Cheep>> GetCheepsFromAuthorByCountAsync(Guid authorId, int count)
    {
        //Use EF to get the specified count of cheeps from an author from the database
        ICollection<Cheep> cheeps = await db
            .Cheeps.Where(c => c.AuthorId == authorId)
            .OrderByDescending(c => c.TimeStamp)
            .Take(count)
            .ToListAsync();

        return cheeps;
    }

    public async Task<int> GetCheepCountAsync()
    {
        //Use EF to get the total number of cheeps from the database
        return await db.Cheeps.CountAsync();
    }

    public async Task<int> GetPageCountAsync()
    {
        return await GetCheepCountAsync() / PageSize + 1;
    }

    public async Task DeleteCheepByIdAsync(Guid cheepId)
    {
        //Delete the specified cheep from the database
        Cheep? cheep = await db.Cheeps.FindAsync(cheepId);
        if (cheep != null)
        {
            db.Cheeps.Remove(cheep);
        }
        else
        {
            throw new Exception("Cheep with id " + cheepId + " not found");
        }

        await db.SaveChangesAsync();
    }

    public async Task AddCheepAsync(Cheep cheep)
    {
        await db.Cheeps.AddAsync(cheep);
        await db.SaveChangesAsync();
        Console.WriteLine("Cheep added async");
    }

    public async Task<Cheep> AddCreateCheepAsync(CreateCheep cheep)
    {
        Cheep entity = new Cheep()
        {
            CheepId = new Guid(),
            Text = cheep.Text,
            TimeStamp = DateTime.Now,
            AuthorId = cheep.AuthorId
        };

        await AddCheepAsync(entity);

        return entity;
    }
}
