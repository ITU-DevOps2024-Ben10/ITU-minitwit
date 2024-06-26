using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web;
using Minitwit.Web.Models;

namespace Minitwit.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    private readonly UserManager<Author> _userManager;
    private readonly IAuthorRepository _authorRepository;
    private readonly SignInManager<Author> _signInManager;

    public ICollection<CheepViewModel>? Cheeps { get; set; }

    public required Author? user { get; set; }
    public required int currentPage { get; set; }
    public required int totalPages { get; set; }

    public UserTimelineModel(
        ICheepService service,
        SignInManager<Author> signInManager,
        UserManager<Author> userManager,
        IAuthorRepository authorRepository
    )
    {
        _service = service;
        _userManager = userManager;
        _authorRepository = authorRepository;
        _signInManager = signInManager;
    }

    public async Task<ActionResult> OnGet(string author)
    {
        user = await _userManager.GetUserAsync(User);

        await InitializeVariables(user!, author);

        return Page();
    }

    public async Task InitializeVariables(Author user, string author)
    {
        if (Request.Query.ContainsKey("page"))
        {
            currentPage = int.Parse(Request.Query["page"]!);
        }
        else
        {
            currentPage = 1;
        }

        Author timelineAuthor = await _authorRepository.GetAuthorByNameAsync(author);

        await LoadCheeps(user, timelineAuthor, currentPage);
    }

    private async Task LoadCheeps(Author signedInAuthor, Author timelineAuthor, int page)
    {
        try
        {
            if (
                _signInManager.IsSignedIn(User)
                && signedInAuthor.UserName == timelineAuthor.UserName
            )
            {
                Cheeps = await _service.GetCheepsFromAuthorAndFollowingAsync(
                    signedInAuthor.Id,
                    page
                );
                totalPages = await _authorRepository.GetPageCountByAuthorAndFollowing(
                    signedInAuthor.Id
                );
            }
            else
            {
                Cheeps = await _service.GetCheepsFromAuthorAsync(timelineAuthor.Id, page);
                totalPages = await _authorRepository.GetPageCountByAuthor(timelineAuthor.Id);
            }
        }
        catch (Exception)
        {
            Cheeps = new List<CheepViewModel>();
        }
    }
}
