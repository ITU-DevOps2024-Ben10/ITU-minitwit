using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;

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

    // Constructor to initialize properties based on an Author entity
    public UserModel(Author author)
    {
        Id = author.Id;
        Username = author.UserName;
        Email = author.Email;
    }
}
