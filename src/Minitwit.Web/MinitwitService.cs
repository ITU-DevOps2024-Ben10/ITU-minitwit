using System.Globalization;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;

namespace Minitwit.Web;

public interface ICheepService
{
    public Task<ICollection<CheepViewModel>> GetCheepsAsync(int page);
    public Task<ICollection<CheepViewModel>> GetCheepsFromAuthor(string authorName, int page);
    public Task<ICollection<CheepViewModel>> GetCheepsFromAuthorAsync(Guid authorId, int page);
    public Task<ICollection<CheepViewModel>> GetCheepsFromAuthorAndFollowingAsync(Guid authorId, int page);

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
    
    public async Task<ICollection<CheepViewModel>> GetCheepsAsync(int page)
    {
        // Fetch cheeps for the given page.
        ICollection<Cheep> cheepDtos = await _cheepRepository.GetCheepsByPageAsync(page);

        // Extract unique author IDs from the cheeps.
        var authorIds = cheepDtos.Select(c => c.AuthorId).Distinct();

        // Fetch only the authors who authored the fetched cheeps.
        ICollection<Author> authors = await _authorRepository.GetAuthorsByIdAsync(authorIds);

        // Initialize a list to hold the tasks for creating CheepViewModels.
        var cheepViewModelTasks = cheepDtos.Select(async cheepDto => 
        {
            // Assuming CheepReactions is updated to be asynchronous
            List<ReactionModel> reactionTypeCounts = await CheepReactionsAsync(cheepDto);
        
            // Find the author for the current cheep.
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);
        
            // Return a new CheepViewModel.
            return new CheepViewModel(
                cheepDto.CheepId, 
                new UserModel(author), 
                cheepDto.Text, 
                cheepDto.TimeStamp.ToString("o"), // Using a round-trip date/time pattern
                reactionTypeCounts
            );
        });

        // Wait for all the CheepViewModel tasks to complete and return the results.
        List<CheepViewModel> cheeps = new List<CheepViewModel>(await Task.WhenAll(cheepViewModelTasks));

        return cheeps;
    }

    
    public async Task<ICollection<CheepViewModel>> GetCheepsFromAuthorAsync(Guid id, int page)
    {
        ICollection<Cheep> cheepDtos = _authorRepository.GetCheepsByAuthor(id, page);
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();
        Author author = _authorRepository.GetAuthorById(id);
        
        foreach (Cheep cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = await CheepReactionsAsync(cheepDto);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }
    
    public async Task<ICollection<CheepViewModel>> GetCheepsFromAuthorAndFollowingAsync(Guid authorId, int page)
    {
        ICollection<Cheep> cheepDtos = _authorRepository.GetCheepsByAuthorAndFollowing(authorId, page);
        ICollection<Author> authors = _authorRepository.GetFollowingById(authorId);
                            authors.Add(_authorRepository.GetAuthorById(authorId));
        ICollection<CheepViewModel> cheeps = new List<CheepViewModel>();

        foreach (Cheep cheepDto in cheepDtos)
        {
            List<ReactionModel> reactionTypeCounts = await CheepReactionsAsync(cheepDto);
            Author? author = authors.FirstOrDefault(a => a.Id == cheepDto.AuthorId);

            cheeps.Add(new CheepViewModel(cheepDto.CheepId, new UserModel(author!), cheepDto.Text, cheepDto.TimeStamp.ToString(CultureInfo.InvariantCulture), reactionTypeCounts));
        }
        
        return cheeps;
    }

    protected async Task<List<ReactionModel>> CheepReactionsAsync(Cheep cheepDto)
    {
        // Initialize reactions with all reaction types set to count 0.
        var reactions = Enum.GetValues(typeof(ReactionType))
            .Cast<ReactionType>()
            .ToDictionary(rt => rt, rt => new ReactionModel(rt, 0));

        // Assume GetReactionsFromCheepIdAsync is an async method.
        ICollection<Reaction> reactionDTOs = await _reactionRepository.GetReactionsFromCheepIdAsync(cheepDto.CheepId);

        // Process the reactions, if any.
        if (reactionDTOs.Any())
        {
            foreach (Reaction reaction in reactionDTOs)
            {
                reactions[reaction.ReactionType].ReactionCount++;
            }
        }

        return reactions.Values.ToList();
    }


    public async Task<ICollection<CheepViewModel>> GetCheepsFromAuthor(string authorName, int page)
    { 
        Author author = await _authorRepository.GetAuthorByNameAsync(authorName);
        var cheeps = await GetCheepsFromAuthorAsync(author.Id, page);
        return cheeps;
    }
}