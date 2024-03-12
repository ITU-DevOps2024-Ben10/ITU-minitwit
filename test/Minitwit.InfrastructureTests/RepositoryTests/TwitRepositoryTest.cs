//Test of cheep repository methods using Test_Utilites in-memory database

using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Test_Utilities;

namespace Minitwit.InfrastructureTest.RepositoryTests;
public class TwitRepositoryTest{

    private readonly TwitRepository _twitRepository;
    private readonly MinitwitDbContext db;

    private readonly Author _author;

    public TwitRepositoryTest()
    {
        db = SqliteInMemoryBuilder.GetContext();
        _twitRepository = new TwitRepository(db);

        _author = new Author()
        {
            UserName = "TestAuthor", 
            Email = "mock@email.com" 
        };
        
        for(int i = 0; i < 34; i++)
        {

            Author authorDto = new Author
            { 
                UserName = "TestAuthor" + i, 
                Email = "mock" + i + "@email.com" 
            };
            
            Twit twitDto = new Twit
            {
                CheepId = Guid.NewGuid(),
                AuthorId = authorDto.Id,
                Text = "TestCheep" + i
            };
            
            db.Users.Add(authorDto);
            db.Cheeps.Add(twitDto);
        }

        db.SaveChanges();
    }

    [Fact]
    public void GetCheepsByPage_ShouldSkipFirst32Cheeps_ReturnXAmountOfCheeps()
    {
        //Act
        ICollection<Twit> cheeps = _twitRepository.GetCheepsByPage(2);

        //Assert
        Assert.Equal(2, cheeps.Count);
    }

    [Fact]
    public void DeleteCheepById_ShouldOnlyDeleteSpecifiedCheep()
    {
        ICollection<Twit> initialCheeps = _twitRepository.GetCheepsByPage(1);
        Twit twit = initialCheeps.First();
        Guid cheepId = twit.CheepId;
        
        _twitRepository.DeleteCheepById(cheepId);

        ICollection<Twit> updatedCheeps = _twitRepository.GetCheepsByPage(1);
        
        //Assert
        Assert.True(initialCheeps.Contains(twit));
        Assert.False(updatedCheeps.Contains(twit));

    }

    [Fact]
    public void addCheep_ShouldAddACheep()
    {
        Twit twitDto = new Twit
        {
            CheepId = Guid.NewGuid(),
            AuthorId = _author.Id,
            Text = "TestCheep",
            TimeStamp = DateTime.Now,
        };

        #pragma warning disable xUnit1031
        _twitRepository.AddCheep(twitDto).Wait();
        #pragma warning restore xUnit1031

        ICollection<Twit> updatedCheeps = _twitRepository.GetCheepsByPage(0);
        
        //Assert
        Assert.True(updatedCheeps.Contains(twitDto));
    }
    
    [Fact]
    public async void CreateCheepCreatesCheep()
    {
        CreateTwit createTwit = new CreateTwit(_author.Id, "TestCheep");

        Twit twit = await _twitRepository.AddCreateCheep(createTwit);
        
        Assert.True(_twitRepository.GetCheepsByPage(0).Contains(twit));
    }
}