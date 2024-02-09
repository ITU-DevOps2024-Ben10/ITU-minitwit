namespace Minitwit.Infrastructure.Repository;

public abstract class BaseRepository
{
    protected MinitwitDbContext db;
    protected int PageSize {get; set;}

    public BaseRepository(MinitwitDbContext minitwitDbContext)
    {
        db = minitwitDbContext;
        PageSize = 32;
    }

}