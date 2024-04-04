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
    private CreateCheep Cheep1;
    private CreateCheep Cheep2;
    private IAuthorRepository authorRepository;
    private ICheepRepository cheepRepository;

    public ChirpDbContextUnitTests()
    {
        Db = SqliteInMemoryBuilder.GetContext();
        authorRepository = new AuthorRepository(Db, new FollowRepository(Db));
        cheepRepository = new CheepRepository(Db);

        // Mock data
        Author1 = new Author { Id = Guid.NewGuid(), UserName = "Author1", Email = "email1" };
        Author2 = new Author { Id = Guid.NewGuid(), UserName = "Author2", Email = "email2" };

        Cheep1 = new CreateCheep(Author1.Id, "Cheep 1");
        Cheep2 = new CreateCheep(Author2.Id, "Cheep 2");
        
        authorRepository.AddAuthorAsync(Author1);
        authorRepository.AddAuthorAsync(Author2);
        
    }
    
    
    [Fact]
    public async void DBContextContainsCheeps()
    {
        await cheepRepository.AddCreateCheep(Cheep1);
        await cheepRepository.AddCreateCheep(Cheep2);
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
        await cheepRepository.AddCreateCheep(Cheep1);
        
        Cheep cheep = Db.Cheeps.FirstOrDefault();

        // Retrieve the Cheep from the database using the Id generated by the repository
        Cheep returnedCheep = await Db.Cheeps.FindAsync(cheep.CheepId);

        Assert.NotNull(returnedCheep);
        Assert.Equal(cheep.CheepId, returnedCheep.CheepId);
        Assert.Equal(cheep.AuthorId, returnedCheep.AuthorId);
        Assert.Equal(cheep.Text, returnedCheep.Text);
        Assert.Equal(cheep.TimeStamp, returnedCheep.TimeStamp, TimeSpan.FromSeconds(1)); // Adjust precision as needed
    }



    [Fact]
    public void QueryByAuthorIdReturnsAuthor()
    {
        cheepRepository.AddCreateCheep(Cheep1);
        cheepRepository.AddCreateCheep(Cheep2);
        
        Db.Cheeps.Include(e => e.AuthorId);
        
        Author? returnedAuthor = Db.Users.Find(Author1.Id);
        
        Assert.NotNull(returnedAuthor);
        Assert.Equal(returnedAuthor.Id, Author1.Id);
        Assert.True(authorRepository.GetCheepsByAuthor(returnedAuthor.Id).Any());
    }
}