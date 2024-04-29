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
    public async void CreateFollow_ShouldCreateFollow()
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

        Follow manuelFollow =
            new()
            {
                FollowingAuthorId = authorThatFollows.Id,
                FollowedAuthorId = authorBeingFollowed.Id
            };

        // Act
        Follow generatedFollow = await followRepository.CreateFollowAsync(
            authorThatFollows.Id,
            authorBeingFollowed.Id
        );

        // Assert
        Assert.Equal(generatedFollow.FollowingAuthorId, manuelFollow.FollowingAuthorId);
        Assert.Equal(generatedFollow.FollowingAuthorId, manuelFollow.FollowingAuthorId);
        Assert.Equal(generatedFollow.FollowedAuthorId, manuelFollow.FollowedAuthorId);
        Assert.Equal(generatedFollow.FollowedAuthorId, manuelFollow.FollowedAuthorId);
    }

    [Fact]
    public async void IsFollowing_ReturnsTrue_WhenUserIsFollowingAnotherUser()
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

        Follow follow =
            new()
            {
                FollowingAuthorId = authorThatFollows.Id,
                FollowedAuthorId = authorBeingFollowed.Id
            };

        context.Follows.Add(follow);
        context.Users.Add(authorThatFollows);
        context.Users.Add(authorBeingFollowed);
        context.SaveChanges();

        // Act


        // Assert
        Assert.True(
            await followRepository.IsFollowingAsync(authorThatFollows.Id, authorBeingFollowed.Id)
        );
    }
}
