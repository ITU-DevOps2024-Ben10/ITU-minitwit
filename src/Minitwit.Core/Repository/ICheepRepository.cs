using Chirp.Core.Entities;

namespace Chirp.Core.Repository;

public interface ICheepRepository
{
    public ICollection<Cheep> GetCheepsByPage(int page);
    public Task AddCheep(Cheep cheepDto);
    public Task<Cheep> AddCreateCheep(CreateCheep cheep);
    public int GetCheepCount();
    public int GetPageCount();

}