using System.ComponentModel.DataAnnotations;
    using Minitwit.Core.Entities;
    using Minitwit.Core.Repository;
using FluentValidation;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
using Minitwit.Web.Models;
using ValidationException = FluentValidation.ValidationException;

    namespace Minitwit.Web.Pages;

    public class PublicModel : PageModel
    {
        private readonly ITwitService _service;
        private readonly ITwitRepository _twitRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IReactionRepository _reactionRepository;
        private readonly IValidator<CreateTwit> _validator;
        public required Author user { get; set; }
        private readonly UserManager<Author> _userManager;
        public required ICollection<CheepViewModel> Cheeps { get; set; }
        public required int totalPages { get; set; }
        public required int currentPage { get; set; }
        
        public PublicModel(ITwitService service, ITwitRepository twitRepository, IAuthorRepository authorRepository, IFollowRepository followRepository, IValidator<CreateTwit> validator , UserManager<Author> userManager, IReactionRepository reactionRepository)

        {
            _service = service;
            _twitRepository = twitRepository;
            _authorRepository = authorRepository;
            _followRepository = followRepository;
            _validator = validator;
            _userManager = userManager;
            _reactionRepository = reactionRepository;
        }

        public ActionResult OnGet()
        {

            InitializeVariables();
            return Page();
        }
        
        [BindProperty] public NewCheep? NewCheep { get; set; }

        public async Task<IActionResult> OnPostCreateCheep()
        {   
            
            if (!ModelState.IsValid)
            { 
                return Page();
            }
           
            var author = await _userManager.GetUserAsync(User);
            var cheep = new CreateTwit(author!.Id, NewCheep!.Text!);

            await CreateCheep(cheep);
            
            return RedirectToPage("/UserTimeline", new { author = User.Identity?.Name });
            
        }
        
        public async Task CreateCheep(CreateTwit newTwit)
        {
            var validationResult = await _validator.ValidateAsync(newTwit);
             
            if (!validationResult.IsValid)
            {
                Console.WriteLine(validationResult);
                //Fatal exception
                throw new ValidationException("The Cheep must be between 5 and 160 characters.(CreateCheep)");
            }

            await _twitRepository.AddCreateCheep(newTwit);
        }
      
        public async Task<IActionResult> OnPostReaction(Guid cheepId, ReactionType reactionType, int currentPage)
        {
           
            Author? author = await _userManager.GetUserAsync(User);
            if (await _reactionRepository.HasUserReacted(cheepId, author!.Id)) return Page();
            await _reactionRepository.AddReaction(reactionType, cheepId, author!.Id);
            InitializeVariables(currentPage);
            return Page();
        }
        public async Task<IActionResult> OnPostRemoveReaction(Guid cheepId, ReactionType reactionType, int currentPage)
        {
            Author? author = await _userManager.GetUserAsync(User);
            if (!await _reactionRepository.HasUserReacted(cheepId, author!.Id)) return Page();
            await _reactionRepository.RemoveReaction(reactionType, cheepId, author!.Id);
            InitializeVariables(currentPage);
            return Page();
        }
        
        
        public async Task<IActionResult> OnPostFollow(int currentPage, Guid Author2Follow)
        {
            
            Author? author = await _authorRepository.GetAuthorByIdAsync(_userManager.GetUserAsync(User).Result!.Id);
            Author? authorToFollow = await _authorRepository.GetAuthorByIdAsync(Author2Follow);
            InitializeVariables(currentPage);



            if (author == null) return Page();

            if (authorToFollow != null) await _authorRepository.AddFollow(author.Id, authorToFollow.Id);
            return Page();
        }
        
        public async Task<IActionResult> OnPostUnfollow(int currentPage, Guid Author2Unfollow)
        {

            Author? author = await _authorRepository.GetAuthorByIdAsync(_userManager.GetUserAsync(User).Result!.Id);
            Author? authorToUnfollow = await _authorRepository.GetAuthorByIdAsync(Author2Unfollow);
            
            InitializeVariables(currentPage);


            if (authorToUnfollow == null || author == null) return Page();

            await _authorRepository.RemoveFollow(author.Id, authorToUnfollow.Id);
            return Page();
        }

        public void InitializeVariables()
        {
            int page;
            if (Request.Query.ContainsKey("page"))
            {
                page = int.Parse(Request.Query["page"]!);
            }
            else
            {
                page = 1;
            }
            InitializeVariables(page);
        }

        public void InitializeVariables(int page)
        {
            Cheeps = _service.GetCheeps(page);

            user = _userManager.GetUserAsync(User).Result!;
            totalPages = _twitRepository.GetPageCount();
            currentPage = page;
        }


    }


    public class NewCheep
    {
        [Required]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "The Cheep must be between 5 and 160 characters(NewCheep).")]
        public string? Text { get; set; }

    }