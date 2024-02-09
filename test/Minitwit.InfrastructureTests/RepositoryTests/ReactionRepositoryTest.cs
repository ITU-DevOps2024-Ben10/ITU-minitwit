using Chirp.Core.Entities;
using Chirp.Infrastructure;
using Minitwit.Infrastructure;
using Test_Utilities;

namespace Chirp.InfrastructureTest.RepositoryTests;

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
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.NewGuid(),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
            Author = authorDto
        };
        db.Users.Add(authorDto);
        db.Cheeps.Add(cheepDto);
        db.SaveChanges(); 
        
        //Act
        await _ReactionRepository.AddReaction(
            reactionType,
            cheepDto.CheepId,
            authorDto.Id
        );

        //Assert
        Assert.Equal(1, cheepDto.Reactions.Count);
        Assert.Equal(cheepDto.CheepId, db.Reactions.First().CheepId);
        Assert.Equal(authorDto.Id, cheepDto.Reactions.First().AuthorId);
        Assert.Equal(reactionType, cheepDto.Reactions.First().ReactionType);
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
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
            Author = authorDto,
            Reactions = new List<Reaction>
            {
                new Reaction
                {
                    CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
                    AuthorId = authorDto.Id,
                    Author = authorDto,
                    ReactionType = reactionType
                }
            }
        };
        db.Users.Add(authorDto);
        db.Cheeps.Add(cheepDto);
        db.SaveChanges(); 
        
        //Act
        await _ReactionRepository.RemoveReaction(
            reactionType,
            cheepDto.CheepId,
            authorDto.Id
        );

        //Assert
        Assert.Equal(0, cheepDto.Reactions.Count);
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
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.NewGuid(),
            AuthorId =authors.First().Id, 
            Text = "TestCheep1", 
            Author = authors.First(),
        };
        db.Cheeps.Add(cheepDto);
        
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
                CheepId = cheepDto.CheepId,
                AuthorId = authors[i].Id,
                Author = authors[i],
                ReactionType = reactionType
            };
            db.Reactions.Add(reaction);
        }
        await db.SaveChangesAsync();
        
        //Act&Assert
        Assert.Equal(1, _ReactionRepository.GetReactionCount(cheepDto.CheepId, ReactionType.Like).Result);
        Assert.Equal(2, _ReactionRepository.GetReactionCount(cheepDto.CheepId, ReactionType.Dislike).Result);
        Assert.Equal(1, _ReactionRepository.GetReactionCount(cheepDto.CheepId, ReactionType.Love).Result);
        
    }
    
    
   
    [Fact]
    public void HasUserReacted_ShouldReturnTrueWhenUserHasReacted()
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
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
            Author = authorDto,
            Reactions = new List<Reaction>
            {
                new Reaction
                {
                    CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
                    AuthorId = authorDto.Id,
                    Author = authorDto,
                    ReactionType = ReactionType.Like
                }
            }
        };
            
        db.Users.Add(authorDto);
        db.Cheeps.Add(cheepDto);
        db.SaveChanges(); 
        
        //Act&Assert
        Assert.True(_ReactionRepository.HasUserReacted(cheepDto.CheepId, authorDto.Id).Result);
        
    }
    [Fact]
    public void HasUserReacted_ShouldReturnFalseWhenUserHasNotReacted()
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
        Cheep cheepDto = new Cheep
        {
            CheepId = Guid.Parse("6e579f4c-c2da-420d-adad-40797a71d217"),
            AuthorId = authorDto1.Id, 
            Text = "TestCheep1", 
            Author = authorDto,
        };
            
        db.Users.Add(authorDto);
        db.Cheeps.Add(cheepDto);
        db.SaveChanges(); 
        
        //Act&Assert
        Assert.False(_ReactionRepository.HasUserReacted(cheepDto.CheepId, authorDto.Id).Result);
        
    }
}