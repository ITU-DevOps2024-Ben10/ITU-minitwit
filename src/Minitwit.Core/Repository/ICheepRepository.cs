using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface ICheepRepository
{
    public ICollection<Cheep> GetCheepsByPage(int page);
    
    public ICollection<Cheep> GetCheepsByCount(int count);
    public Task AddCheep(Cheep cheepDto);
    public Task<Cheep> AddCreateCheep(CreateCheep cheep);
    public int GetCheepCount();
    public int GetPageCount();

}