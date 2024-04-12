// Pages/AboutMe.cshtml.cs

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;

namespace Minitwit.Web.Areas.Identity.Pages.Account;

public class AboutMeModel : PageModel
{
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;
    private readonly ICheepService _service;
    private IAuthorRepository _authorRepository;
    private ICheepRepository _cheepRepository;

    //
    public UserModel? UserModel { get; set; }
    public ICollection<CheepViewModel>? Cheeps { get; set; }
    public ICollection<Author?>? Followers { get; set; }
    public ICollection<Author?>? Following { get; set; }

    // This is the user that the _CheepList is expected to find to create the cheeps
    public Author? user { get; set; }
    public required int currentPage { get; set; }
    public required int totalPages { get; set; }

    public AboutMeModel(
        UserManager<Author> userManager,
        SignInManager<Author> signInManager,
        ICheepService service,
        IAuthorRepository authorRepository,
        ICheepRepository cheepRepository
    )
    {
        _userManager = userManager;
        _service = service;
        _authorRepository = authorRepository;
        _cheepRepository = cheepRepository;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Fetch user information from the database
        user = await _userManager.GetUserAsync(User);

        // Create a UserModel based on the Author entity
        UserModel = new UserModel(user!);

        // Retrieve the followers and following of the user

        Followers = await _authorRepository.GetFollowersByIdAsync(user!.Id);
        Following = await _authorRepository.GetFollowingByIdAsync(user!.Id);

        if (Request.Query.ContainsKey("page"))
        {
            currentPage = int.Parse(Request.Query["page"]!);
        }
        else
        {
            currentPage = 1;
        }

        try
        {
            Cheeps = await _service.GetCheepsFromAuthorAsync(UserModel.Id, currentPage);
        }
        catch (Exception)
        {
            Cheeps = new List<CheepViewModel>();
        }

        totalPages = await _authorRepository.GetPageCountByAuthor(UserModel.Id);

        return Page();
    }

    // Forget me method
    public async Task<IActionResult> OnPostDeleteMe()
    {
        // Check if the user is authenticated
        if (!User.Identity!.IsAuthenticated)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // Fetch user information from the database
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage();
        }

        // Get the logins for the user
        var logins = await _userManager.GetLoginsAsync(user);

        // Remove each login
        foreach (var login in logins)
        {
            var result = await _userManager.RemoveLoginAsync(
                user,
                login.LoginProvider,
                login.ProviderKey
            );
            if (!result.Succeeded)
            {
                // Handle error
                throw new InvalidOperationException(
                    $"Unexpected error occurred removing external login for user with ID '{user.Id}'."
                );
            }
        }

        if (_authorRepository != null)
        {
            await _authorRepository.DeleteCheepsByAuthorIdAsync(user.Id);
            await _authorRepository.RemoveAllFollowersByAuthorIdAsync(user.Id);
            await _authorRepository.RemoveReactionsByAuthorIdAsync(user.Id);
            await _authorRepository.RemoveUserByIdAsync(user.Id);
            await _authorRepository.SaveContextAsync();
        }
        else
        {
            return BadRequest("_authorRepository is null.");
        }

        // log out the user
        if (_signInManager != null)
        {
            await _signInManager.SignOutAsync();
        }
        else
        {
            return BadRequest("_signInManager is null.");
        }

        // Redirect to the start page
        return RedirectToPage("/Public");
    }
}
