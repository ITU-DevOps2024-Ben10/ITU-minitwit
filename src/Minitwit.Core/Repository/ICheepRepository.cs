using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface ICheepRepository
{
    public ICollection<Cheep> GetCheepsByPage(int page);
    
    public Task<ICollection<Cheep>> GetCheepsByCountAsync(int count);


    public ICollection<Cheep> GetCheepsFromAuthorByCount(Guid authorId, int count);

  
    public Task AddCheep(Cheep cheepDto);
    public Task<Cheep> AddCreateCheepAsync(CreateCheep cheep);
    public int GetCheepCount();
    public int GetPageCount();

}