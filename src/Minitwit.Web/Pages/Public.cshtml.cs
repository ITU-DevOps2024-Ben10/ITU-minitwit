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
        private readonly ICheepService _service;
        private readonly ICheepRepository _cheepRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IReactionRepository _reactionRepository;
        private readonly IValidator<CreateCheep> _validator;
        public required Author user { get; set; }
        private readonly UserManager<Author> _userManager;
        public required ICollection<CheepViewModel> Cheeps { get; set; }
        public required int totalPages { get; set; }
        public required int currentPage { get; set; }
        
        public PublicModel(ICheepService service, ICheepRepository cheepRepository, IAuthorRepository authorRepository, IFollowRepository followRepository, IValidator<CreateCheep> validator , UserManager<Author> userManager, IReactionRepository reactionRepository)

        {
            _service = service;
            _cheepRepository = cheepRepository;
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
            var cheep = new CreateCheep(author!, NewCheep!.Text!);

            await CreateCheep(cheep);
            
            return RedirectToPage("/UserTimeline", new { author = User.Identity?.Name });
            
        }
        
        public async Task CreateCheep(CreateCheep newCheep)
        {
            var validationResult = await _validator.ValidateAsync(newCheep);
             
            if (!validationResult.IsValid)
            {
                Console.WriteLine(validationResult);
                //Fatal exception
                throw new ValidationException("The Cheep must be between 5 and 160 characters.(CreateCheep)");
            }

            await _cheepRepository.AddCreateCheep(newCheep);
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

            await _authorRepository.AddFollow(author, authorToFollow);
            return Page();
        }
        
        public async Task<IActionResult> OnPostUnfollow(int currentPage, Guid Author2Unfollow)
        {

            Author? author = await _authorRepository.GetAuthorByIdAsync(_userManager.GetUserAsync(User).Result!.Id);
            Author? authorToUnfollow = await _authorRepository.GetAuthorByIdAsync(Author2Unfollow);
            
            InitializeVariables(currentPage);


            if (authorToUnfollow == null || author == null) return Page();

            await _authorRepository.RemoveFollow(author!, authorToUnfollow);
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
            totalPages = _cheepRepository.GetPageCount();
            currentPage = page;
        }


    }


    public class NewCheep
    {
        [Required]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "The Cheep must be between 5 and 160 characters(NewCheep).")]
        public string? Text { get; set; }

    }