using Minitwit.Core.Entities;

namespace Minitwit.Core.Repository;

public interface ITwitRepository
{
    public ICollection<Twit> GetCheepsByPage(int page);
    
    public ICollection<Twit> GetCheepsByCount(int count);


    public ICollection<Twit> GetCheepsFromAuthorByCount(Guid authorId, int count);

  
    public Task AddCheep(Twit twitDto);
    public Task<Twit> AddCreateCheep(CreateTwit twit);
    public int GetCheepCount();
    public int GetPageCount();

}