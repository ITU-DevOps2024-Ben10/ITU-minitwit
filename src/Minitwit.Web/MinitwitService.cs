using System.Globalization;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;

namespace Minitwit.Web;

public interface ITwitService
{
    public ICollection<CheepViewModel> GetCheeps(int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthor(string authorName, int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthor(Guid authorId, int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthorAndFollowing(Guid authorId, int page);

}

public class MinitwitService : ITwitService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ITwitRepository _twitRepository;
    private readonly IReactionRepository _reactionRepository;
    
    public MinitwitService(ITwitRepository twitRepository, IAuthorRepository authorRepository, IReactionRepository reactionRepository)
    {
        _twitRepository = twitRepository;
        _authorRepository = authorRepository;
        _reactionRepository = reactionRepository;
    }
    
    public ICollection<CheepViewModel> GetCheeps(int page)
    {
        ICollection<Twit> cheepDtos = _twitRepository.GetCheepsByPage(page);
        List<CheepViewModel> cheeps = new List<CheepViewModel>();
        ICollection<Author> authors = _authorRepository.GetAllAuthors();

        foreach (Twit cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);
            
            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }

        return cheeps;
    }
    
    public ICollection<CheepViewModel> GetCheepsFromAuthor(Guid id, int page)
    {
        ICollection<Twit> cheepDtos = _authorRepository.GetCheepsByAuthor(id, page);
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();
        Author author = _authorRepository.GetAuthorById(id);
        
        foreach (Twit cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }
    
    public ICollection<CheepViewModel> GetCheepsFromAuthorAndFollowing(Guid authorId, int page)
    {
        ICollection<Twit> cheepDtos = _authorRepository.GetCheepsByAuthorAndFollowing(authorId, page);
        ICollection<Author> authors = _authorRepository.GetFollowingById(authorId);
                            authors.Add(_authorRepository.GetAuthorById(authorId));
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();

        foreach (Twit cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author!), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }

    protected List<ReactionModel> CheepReactions(Twit twitDto)
    {
        // Initialize reactions with all reaction types set to count 0.
        var reactions = Enum.GetValues(typeof(ReactionType))
            .Cast<ReactionType>()
            .ToDictionary(rt => rt, rt => new ReactionModel(rt, 0));

        ICollection<Reaction> reactionDTOs = _reactionRepository.GetReactionsFromCheepId(twitDto.CheepId);
        // If cheepDto.Reactions is not null and has elements, process them.
        if (reactionDTOs.Any() == true)
        {
            foreach (Reaction reaction in reactionDTOs)
            {
                reactions[reaction.ReactionType].ReactionCount++;
            }
        }

        return reactions.Values.ToList();
    }

    public ICollection<CheepViewModel> GetCheepsFromAuthor(string authorName, int page)
    {
        var author = _authorRepository.GetAuthorByName(authorName);
        var cheeps = GetCheepsFromAuthor(author.Id, page);
        return cheeps;
    }
}