using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface ICheepRepository
{
    public Task<ICollection<Cheep>> GetCheepsByPageAsync(int page);
    
    public Task<ICollection<Cheep>> GetCheepsByCountAsync(int count);


    public Task<ICollection<Cheep>> GetCheepsFromAuthorByCountAsync(Guid authorId, int count);

  
    public Task AddCheepAsync(Cheep cheepDto);
    public Task<Cheep> AddCreateCheepAsync(CreateCheep cheep);
    public Task<int> GetCheepCountAsync();
    public Task<int> GetPageCountAsync();

}