using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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



    public UserTimelineModel(ICheepService service, SignInManager<Author> signInManager, UserManager<Author> userManager, IAuthorRepository authorRepository)
    {
        _service = service;
        _userManager = userManager;
        _authorRepository = authorRepository;
        _signInManager = signInManager;
    }

    public ActionResult OnGet(string author)
    {
        user = _userManager.GetUserAsync(User).Result;

        InitializeVariables(user!, author);

        return Page();
    }


    public async void InitializeVariables(Author user, string author)
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

        LoadCheeps(user, timelineAuthor, currentPage);
    }

    private void LoadCheeps(Author signedInAuthor, Author timelineAuthor, int page)
    {
        try
        {
            if (_signInManager.IsSignedIn(User) && signedInAuthor.UserName == timelineAuthor.UserName)
            {
                Cheeps = _service.GetCheepsFromAuthorAndFollowing(signedInAuthor.Id, page);
                totalPages = _authorRepository.GetPageCountByAuthorAndFollowing(signedInAuthor.Id);

            }
            else
            {
                Cheeps = _service.GetCheepsFromAuthor(timelineAuthor.Id, page);
                totalPages = _authorRepository.GetPageCountByAuthor(timelineAuthor.Id);
            }
        }
        catch (Exception)
        {
            Cheeps = new List<CheepViewModel>();
        }
    }
}