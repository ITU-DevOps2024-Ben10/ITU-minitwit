//Test of cheep repository methods using Test_Utilites in-memory database

using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Test_Utilities;

namespace Minitwit.InfrastructureTest.RepositoryTests;

public class CheepRepositoryTest
{
    private readonly CheepRepository CheepRepository;
    private readonly MinitwitDbContext db;

    private readonly Author _author;

    public CheepRepositoryTest()
    {
        db = SqliteInMemoryBuilder.GetContext();
        CheepRepository = new CheepRepository(db);

        _author = new Author() { UserName = "TestAuthor", Email = "mock@email.com" };

        for (int i = 0; i < 34; i++)
        {
            Author authorDto = new Author
            {
                UserName = "TestAuthor" + i,
                Email = "mock" + i + "@email.com"
            };

            Cheep cheepDto = new Cheep
            {
                CheepId = Guid.NewGuid(),
                AuthorId = authorDto.Id,
                Text = "TestCheep" + i
            };

            db.Users.Add(authorDto);
            db.Cheeps.Add(cheepDto);
        }

        db.SaveChanges();
    }

    [Fact]
    public async void GetCheepsByPage_ShouldSkipFirst32Cheeps_ReturnXAmountOfCheeps()
    {
        //Act
        ICollection<Cheep> cheeps = await CheepRepository.GetCheepsByPageAsync(2);

        //Assert
        Assert.Equal(2, cheeps.Count);
    }

    [Fact]
    public async void DeleteCheepById_ShouldOnlyDeleteSpecifiedCheep()
    {
        ICollection<Cheep> initialCheeps = await CheepRepository.GetCheepsByPageAsync(1);
        Cheep cheep = initialCheeps.First();
        Guid cheepId = cheep.CheepId;

        CheepRepository.DeleteCheepByIdAsync(cheepId);

        ICollection<Cheep> updatedCheeps = await CheepRepository.GetCheepsByPageAsync(1);

        //Assert
        Assert.True(initialCheeps.Contains(cheep));
        Assert.False(updatedCheeps.Contains(cheep));
    }

    [Fact]
    public async void addCheep_ShouldAddACheep()
    {
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.NewGuid(),
            AuthorId = _author.Id,
            Text = "TestCheep",
            TimeStamp = DateTime.Now,
        };

#pragma warning disable xUnit1031
        CheepRepository.AddCheepAsync(cheepDto).Wait();
#pragma warning restore xUnit1031

        ICollection<Cheep> updatedCheeps = await CheepRepository.GetCheepsByPageAsync(0);

        //Assert
        Assert.True(updatedCheeps.Contains(cheepDto));
    }

    [Fact]
    public async void CreateCheepCreatesCheep()
    {
        CreateCheep createCheep = new CreateCheep(_author.Id, "TestCheep");

        Cheep cheep = await CheepRepository.AddCreateCheepAsync(createCheep);

        ICollection<Cheep> cheeps = await CheepRepository.GetCheepsByPageAsync(0);

        Assert.True(cheeps.Contains(cheep));
    }
}
