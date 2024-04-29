using Minitwit.Core.Entities;

namespace Minitwit.Web.Models;

public record CheepViewModel(
    Guid CheepId,
    UserModel User,
    string Message,
    string Timestamp,
    ICollection<ReactionModel> Reactions
);
