using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Web.Models;

/// <summary>
/// This class represents a model of a user,
/// and is used in the frontend.
/// </summary>

public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public ICollection<Follow> Followers { get; set; }
    public ICollection<Follow> Following { get; set; }
    
    // Constructor to initialize properties based on an Author entity
    public UserModel(Author author)
    {
        Id = author.Id;
        Username = author.UserName;
        Email = author.Email;
        Followers = author.Followers;
        Following = author.Following;
    }

    public bool UserHasFollowers(Guid userId)
    {
        // check if id is in followers
        return Following.Any(Follow => Follow.FollowingAuthor!.Id == userId);
    }
    
    public bool UserIsFollowing(Guid followedUser)
    {
        if (Following != null && Following.Any())
        {
            return true;
        }

        return false;
    }

    public bool IsFollowing(Guid followedUserId)
    {
        return Following.Any(f => f.FollowedAuthor!.Id == followedUserId);
    }   
}