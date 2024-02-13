using Chirp.Core.Entities;

namespace Chirp.Web.Models;

public record CheepViewModel(Guid CheepId, UserModel User, string Message, string Timestamp, ICollection<ReactionModel> Reactions);