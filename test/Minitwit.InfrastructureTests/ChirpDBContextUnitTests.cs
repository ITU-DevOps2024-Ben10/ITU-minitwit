﻿using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Test_Utilities;

namespace Minitwit.InfrastructureTest;

public class ChirpDbContextUnitTests
{

    private readonly MinitwitDbContext Db;
    private readonly Author Author1;
    private readonly Author Author2;
    private CreateTwit Cheep1;
    private CreateTwit Cheep2;
    private IAuthorRepository authorRepository;
    private ITwitRepository _twitRepository;

    public ChirpDbContextUnitTests()
    {
        Db = SqliteInMemoryBuilder.GetContext();
        authorRepository = new AuthorRepository(Db);
        _twitRepository = new TwitRepository(Db);

        // Mock data
        Author1 = new Author { Id = Guid.NewGuid(), UserName = "Author1", Email = "email1" };
        Author2 = new Author { Id = Guid.NewGuid(), UserName = "Author2", Email = "email2" };

        Cheep1 = new CreateTwit(Author1.Id, "Cheep 1");
        Cheep2 = new CreateTwit(Author2.Id, "Cheep 2");
        
        authorRepository.AddAuthor(Author1);
        authorRepository.AddAuthor(Author2);
        
    }
    
    
    [Fact]
    public async void DBContextContainsCheeps()
    {
        await _twitRepository.AddCreateCheep(Cheep1);
        await _twitRepository.AddCreateCheep(Cheep2);
        Assert.True(Db.Cheeps.Any());
    }

    [Fact]
    public void DBContextContainsAuthors()
    {
        Assert.True(Db.Users.Any());
    }

    [Fact]
    public async Task QueryByCheepIdReturnsCheep()
    {
        Db.Cheeps.RemoveRange(Db.Cheeps);
        
        // Add Cheep1 to the repository, and let the repository generate the Id
        await _twitRepository.AddCreateCheep(Cheep1);
        
        Twit twit = Db.Cheeps.FirstOrDefault();

        // Retrieve the Cheep from the database using the Id generated by the repository
        Twit returnedTwit = await Db.Cheeps.FindAsync(twit.CheepId);

        Assert.NotNull(returnedTwit);
        Assert.Equal(twit.CheepId, returnedTwit.CheepId);
        Assert.Equal(twit.AuthorId, returnedTwit.AuthorId);
        Assert.Equal(twit.Text, returnedTwit.Text);
        Assert.Equal(twit.TimeStamp, returnedTwit.TimeStamp, TimeSpan.FromSeconds(1)); // Adjust precision as needed
    }



    [Fact]
    public void QueryByAuthorIdReturnsAuthor()
    {
        _twitRepository.AddCreateCheep(Cheep1);
        _twitRepository.AddCreateCheep(Cheep2);
        
        Db.Cheeps.Include(e => e.AuthorId);
        
        Author? returnedAuthor = Db.Users.Find(Author1.Id);
        
        Assert.NotNull(returnedAuthor);
        Assert.Equal(returnedAuthor.Id, Author1.Id);
        Assert.True(authorRepository.GetCheepsByAuthor(returnedAuthor.Id).Any());
    }
}