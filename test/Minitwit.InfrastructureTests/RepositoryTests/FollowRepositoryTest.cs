using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Test_Utilities;

namespace Minitwit.InfrastructureTest.RepositoryTests;

public class FollowRepositoryTest
{
    private readonly MinitwitDbContext context;

    public FollowRepositoryTest()
    {
        context = SqliteInMemoryBuilder.GetContext();
    }

    [Fact]
    public void CreateFollow_ShouldCreateFollow()
    {
        // Arrange
        FollowRepository followRepository = new(context);
        
        Author? authorThatFollows = new Author()
        {
            Id = Guid.NewGuid(),
            UserName = "authorThatFollows",
            Email = "Follower@mail.com",
        };
        Author? authorBeingFollowed = new Author()
        {
            Id = Guid.NewGuid(),
            UserName = "authorBeingFollowed",
            Email = "Following@mail.com"
        };
        
        Follow manuelFollow = new()
        {
            FollowingAuthor = authorThatFollows,
            FollowingAuthorId = authorThatFollows.Id,
            FollowedAuthor = authorBeingFollowed,
            FollowedAuthorId = authorBeingFollowed.Id
        };
        
        // Act
        Follow generatedFollow = followRepository.CreateFollow(authorThatFollows, authorBeingFollowed);
        
        // Assert
        Assert.Equal(generatedFollow.FollowingAuthor, manuelFollow.FollowingAuthor);
        Assert.Equal(generatedFollow.FollowingAuthorId, manuelFollow.FollowingAuthorId);
        Assert.Equal(generatedFollow.FollowedAuthor, manuelFollow.FollowedAuthor);
        Assert.Equal(generatedFollow.FollowedAuthorId, manuelFollow.FollowedAuthorId);
    }

    [Fact]
    public void IsFollowing_ReturnsTrue_WhenUserIsFollowingAnotherUser()
    {
        // Arrange
        FollowRepository followRepository = new(context);

        Author? authorThatFollows = new Author()
        {
            Id = Guid.NewGuid(),
            UserName = "authorThatFollows",
            Email = "Follower@mail.com",
        };
        Author? authorBeingFollowed = new Author()
        {
            Id = Guid.NewGuid(),
            UserName = "authorBeingFollowed",
            Email = "Following@mail.com"
        };
        
        Follow follow = new()
        {
            FollowingAuthor = authorThatFollows,
            FollowingAuthorId = authorThatFollows.Id,
            FollowedAuthor = authorBeingFollowed,
            FollowedAuthorId = authorBeingFollowed.Id
        };
        
        context.Follows.Add(follow);
        context.Users.Add(authorThatFollows);
        context.Users.Add(authorBeingFollowed);
        context.SaveChanges();
        
        // Act
        

        // Assert
        Assert.True(followRepository.IsFollowing(authorThatFollows.Id, authorBeingFollowed.Id));

    }
}