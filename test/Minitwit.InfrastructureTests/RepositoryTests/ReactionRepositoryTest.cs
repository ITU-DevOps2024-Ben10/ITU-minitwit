using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Test_Utilities;

namespace Minitwit.InfrastructureTest.RepositoryTests;

public class ReactionRepositoryTest
{
    private ReactionRepository _ReactionRepository;
    private readonly MinitwitDbContext db;

    public ReactionRepositoryTest()
    {
        db = SqliteInMemoryBuilder.GetContext();
    }
    
    [Theory]
    [InlineData(ReactionType.Like)]
    [InlineData(ReactionType.Dislike)]
    [InlineData(ReactionType.Love)]
    public async Task AddReaction_ShouldAddReactionToCheep(ReactionType reactionType)
    {
        //Arrange
        _ReactionRepository = new ReactionRepository(db);
        
        Author authorDto = new Author
        {
            UserName = "TestAuthor1", 
            Email = "mock1@email.com"
        };
        Author authorDto1 = new Author
        { 
                UserName = "TestAuthor2", 
                Email = "mock2@email.com" 
            };
        Twit twitDto = new Twit
        {
            CheepId = Guid.NewGuid(),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
        };
        db.Users.Add(authorDto);
        db.Cheeps.Add(twitDto);
        await db.SaveChangesAsync(); 
        
        //Act
        await _ReactionRepository.AddReaction(
            reactionType,
            twitDto.CheepId,
            authorDto.Id
        );

        //Assert
        Assert.Equal(1, db.Reactions.Count(reaction => reaction.CheepId == twitDto.CheepId));
        //Assert.Equal(1, cheepDto.Reactions.Count);
        Assert.Equal(twitDto.CheepId, db.Reactions.First().CheepId);
        Assert.Equal(authorDto.Id, db.Reactions.FirstOrDefault(reaction => reaction.CheepId == twitDto.CheepId)!.AuthorId);
        Assert.Equal(reactionType, db.Reactions.FirstOrDefault(reaction => reaction.CheepId == twitDto.CheepId)!.ReactionType);
        //Assert.Equal(authorDto.Id, cheepDto.Reactions.First().AuthorId);
        //Assert.Equal(reactionType, cheepDto.Reactions.First().ReactionType);
    }
    
    [Theory]
    [InlineData(ReactionType.Like)]
    [InlineData(ReactionType.Dislike)]
    [InlineData(ReactionType.Love)]
    public async Task RemoveReaction_ShouldAddRemoveReactionFromCheep(ReactionType reactionType)
    {
        //Arrange
        _ReactionRepository = new ReactionRepository(db);
        
        Author authorDto = new Author
        {
            UserName = "TestAuthor1", 
            Email = "mock1@email.com"
        };
        Author authorDto1 = new Author
        { 
            UserName = "TestAuthor2", 
            Email = "mock2@email.com" 
        };
        Twit twitDto = new Twit
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1"
        };
        db.Users.Add(authorDto);
        db.Cheeps.Add(twitDto);
        await db.SaveChangesAsync(); 
        
        //Act
        await _ReactionRepository.RemoveReaction(
            reactionType,
            twitDto.CheepId,
            authorDto.Id
        );

        //Assert
        Assert.Equal(0, db.Reactions.Count(reaction => reaction.CheepId == twitDto.CheepId));
        // Assert.Equal(0, cheepDto.Reactions.Count);
    }
    [Fact]
    public async Task GetReactionCount_ShouldReturnTheCorrectAmountOfReactions()
    {
        //Arrange
        _ReactionRepository = new ReactionRepository(db);
        
        //List inorder to use specific authors when adding reactions
        List<Author> authors = new();
        for (int i = 0; i < 4; i++)
        {
            Author authorDto = new Author
            {
                UserName = "TestAuthor" + i, 
                Email = "mock"+ i + "@email.com"
            };
            db.Users.Add(authorDto);
            authors.Add(authorDto);
        }
       
        //Cheep to be reacted to
        Twit twitDto = new Twit
        {
            CheepId = Guid.NewGuid(),
            AuthorId =authors.First().Id, 
            Text = "TestCheep1", 
        };
        db.Cheeps.Add(twitDto);
        
        //Adding reactions to cheep, 1 likes, 2 dislike, 1 love 
        for (int i = 0; i < 4; i++)
        {
            ReactionType reactionType = ReactionType.Like; 
            if (i%2 == 0)
            {
                reactionType = ReactionType.Dislike;
            } else if (i%3 == 0)
            {
                reactionType = ReactionType.Love;
            }
            else if(i == 0 || i == 1)
            {
                reactionType = ReactionType.Like;
            }
            
            Reaction reaction = new Reaction
            {
                CheepId = twitDto.CheepId,
                AuthorId = authors[i].Id,
                ReactionType = reactionType
            };
            db.Reactions.Add(reaction);
        }
        await db.SaveChangesAsync();
        
        //Act&Assert
        Assert.Equal(1, await _ReactionRepository.GetReactionCount(twitDto.CheepId, ReactionType.Like));
        Assert.Equal(2, await _ReactionRepository.GetReactionCount(twitDto.CheepId, ReactionType.Dislike));
        Assert.Equal(1, await _ReactionRepository.GetReactionCount(twitDto.CheepId, ReactionType.Love));
        
    }
    
    
   
    [Fact]
    public async void HasUserReacted_ShouldReturnTrueWhenUserHasReacted()
    {
        //Arrange
        _ReactionRepository = new ReactionRepository(db);
        
        Author authorDto = new Author
        {
            UserName = "TestAuthor1", 
            Email = "mock1@email.com"
        };
        Author authorDto1 = new Author
        { 
            UserName = "TestAuthor2", 
            Email = "mock2@email.com" 
        };
        Twit twitDto = new Twit
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
        };
            
        db.Users.Add(authorDto);
        db.Cheeps.Add(twitDto);
        await _ReactionRepository.AddReaction(ReactionType.Like, twitDto.CheepId, authorDto.Id);
        
        await db.SaveChangesAsync(); 
        
        //Act
        bool hasReacted = await _ReactionRepository.HasUserReacted(twitDto.CheepId, authorDto.Id);
        
        //Assert
        Assert.True(hasReacted);
        
    }
    [Fact]
    public async void HasUserReacted_ShouldReturnFalseWhenUserHasNotReacted()
    {
        //Arrange
        _ReactionRepository = new ReactionRepository(db);

        Author authorDto = new Author
        {
            UserName = "TestAuthor1", 
            Email = "mock1@email.com"
        };
        Author authorDto1 = new Author
        { 
            UserName = "TestAuthor2", 
            Email = "mock2@email.com" 
        };
        Twit twitDto = new Twit
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
        };
            
        db.Users.Add(authorDto);
        db.Cheeps.Add(twitDto);
        db.SaveChanges(); 
        
        //Act&Assert
        Assert.False(await _ReactionRepository.HasUserReacted(twitDto.CheepId, authorDto.Id));
        
    }
}