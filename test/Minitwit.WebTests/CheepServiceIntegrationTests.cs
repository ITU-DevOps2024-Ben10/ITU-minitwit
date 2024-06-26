﻿using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Minitwit.Web;
using Minitwit.Web.Models;
using Test_Utilities;

namespace Minitwit.WebTests;

public class CheepServiceIntegrationTests
{
    private MinitwitService _service;

    private readonly Author _author1;
    private readonly Author _author2;
    private readonly Cheep _cheep1;
    private readonly Cheep _cheep2;
    private readonly Cheep _cheep3;

    public CheepServiceIntegrationTests()
    {
        MinitwitDbContext context = SqliteInMemoryBuilder.GetContext();
        IFollowRepository followRepository = new FollowRepository(context);
        ICheepRepository cheepRepository = new CheepRepository(context);
        IAuthorRepository authorRepository = new AuthorRepository(
            context,
            new FollowRepository(context)
        );
        IReactionRepository reactionRepository = new ReactionRepository(context);
        _service = new MinitwitService(cheepRepository, authorRepository, reactionRepository);

        _author1 = new Author
        {
            Id = Guid.NewGuid(),
            UserName = "Author1",
            Email = "email1"
        };
        _author2 = new Author
        {
            Id = Guid.NewGuid(),
            UserName = "Author2",
            Email = "email2"
        };

        _cheep1 = new Cheep
        {
            CheepId = new Guid(),
            AuthorId = _author1.Id,
            Text = "Cheep 1",
            TimeStamp = DateTime.Now,
        };

        _cheep2 = new Cheep
        {
            CheepId = new Guid(),
            AuthorId = _author2.Id,
            Text = "Cheep 2",
            TimeStamp = DateTime.Now,
        };

        _cheep3 = new Cheep()
        {
            CheepId = new Guid(),
            AuthorId = _author2.Id,
            Text = "Cheep 3",
            TimeStamp = DateTime.Now,
        };

        Follow f = followRepository.CreateFollowAsync(_author1.Id, _author2.Id).Result;

        context.Add(_author1);
        context.Add(_author2);
        context.Add(_cheep1);
        context.Add(_cheep2);
        context.Add(_cheep3);
        context.Add(f);

        context.SaveChanges();
    }

    /*
    [Fact]
    public async void GetCheeps_ReturnsCheepViewModels()
    {
        // Act
        List<CheepViewModel> result = _service.GetCheepsAsync(0).Result.ToList();

        result.Sort((a, b) => String.Compare(a.User.Username, b.User.Username, StringComparison.Ordinal));
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Author1", result[0].User.Username);
        Assert.Equal("Cheep 1", result[0].Message);
        Assert.NotNull(result[0].Timestamp);
    }*/


    /*
    [Fact]
    public void GetCheepsFromAuthor_ReturnsCheepViewModels()
    {
        // Act
        ICollection<CheepViewModel> result = _service.GetCheepsFromAuthor(_author2.Id, 1);

        CheepViewModel returnedCheep = result.ElementAt(0);
    
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(_author2.UserName, returnedCheep.User.Username);
        Assert.Equal("Cheep 3", returnedCheep.Message);
        Assert.NotNull(returnedCheep.Timestamp);
    }
    */

    /*
    [Fact]
    public void GetCheepsFromAuthorAndFollowing_returnsCheepsFromAuthorAndFollowingAuthor()
    {
        //act
        ICollection<CheepViewModel> result = _service.GetCheepsFromAuthorAndFollowing(_author1.Id, 1);
        
        //assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Author2", result.ElementAt(0).User.Username);
        Assert.Equal("Author2", result.ElementAt(1).User.Username);
        Assert.Equal("Author1", result.ElementAt(2).User.Username);
        
    }
    */
}
