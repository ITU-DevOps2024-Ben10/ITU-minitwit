using Chirp.Web.Models;

namespace ITU_minitwit.Models;

public record CheepViewModel(Guid CheepId, UserModel User, string Message, string Timestamp, ICollection<ReactionModel> Reactions);