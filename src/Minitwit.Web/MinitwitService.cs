using System.Globalization;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;

namespace Minitwit.Web;

public interface ICheepService
{
    public ICollection<CheepViewModel> GetCheeps(int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthor(string authorName, int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthor(Guid authorId, int page);
    public ICollection<CheepViewModel> GetCheepsFromAuthorAndFollowing(Guid authorId, int page);

}

public class MinitwitService : ICheepService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICheepRepository _cheepRepository;
    private readonly IReactionRepository _reactionRepository;
    
    public MinitwitService(ICheepRepository cheepRepository, IAuthorRepository authorRepository, IReactionRepository reactionRepository)
    {
        _cheepRepository = cheepRepository;
        _authorRepository = authorRepository;
        _reactionRepository = reactionRepository;
    }
    
    public ICollection<CheepViewModel> GetCheeps(int page)
    {
        ICollection<Cheep> cheepDtos = _cheepRepository.GetCheepsByPage(page);
        List<CheepViewModel> cheeps = new List<CheepViewModel>();
        ICollection<Author> authors = _authorRepository.GetAuthors();

        foreach (Cheep cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);
            
            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }

        return cheeps;
    }
    
    public ICollection<CheepViewModel> GetCheepsFromAuthor(Guid id, int page)
    {
        ICollection<Cheep> cheepDtos = _authorRepository.GetCheepsByAuthor(id, page);
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();
        Author author = _authorRepository.GetAuthorById(id);
        
        foreach (Cheep cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }
    
    public ICollection<CheepViewModel> GetCheepsFromAuthorAndFollowing(Guid authorId, int page)
    {
        ICollection<Cheep> cheepDtos = _authorRepository.GetCheepsByAuthorAndFollowing(authorId, page);
        ICollection<Author> authors = _authorRepository.GetFollowingById(authorId);
                            authors.Add(_authorRepository.GetAuthorById(authorId));
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();

        foreach (Cheep cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = CheepReactions(cheepDto);
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author!), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }

    protected List<ReactionModel> CheepReactions(Cheep cheepDto)
    {
        // Initialize reactions with all reaction types set to count 0.
        var reactions = Enum.GetValues(typeof(ReactionType))
            .Cast<ReactionType>()
            .ToDictionary(rt => rt, rt => new ReactionModel(rt, 0));

        ICollection<Reaction> reactionDTOs = _reactionRepository.GetReactionsFromCheepId(cheepDto.CheepId);
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