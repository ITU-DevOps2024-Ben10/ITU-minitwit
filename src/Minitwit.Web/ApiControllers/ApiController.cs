using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;
using Minitwit.Web.Areas.Identity.Pages.Account;

/*
 * TODO REMOVE THIS COMMENT WHEN THE API IS DONE
 *
 * This is a rough draft of our API, logic its not perfect and it is not complete.
 * For now we're just experimenting with the API and how it should be structured.
 */
namespace Minitwit.Web.ApiControllers;

//Isn't needed as the endpoints we need to expose doesn't clash with any endpoints we already use
[Route("api")]
[ApiController]
public class ApiController : ControllerBase

{
    private readonly ICheepService _cheepService;
    private readonly IAuthorRepository _authorRepository;
    private readonly ICheepRepository _cheepRepository;
    private readonly UserManager<Author> _userManager;
    private readonly IUserStore<Author> _userStore;
    private readonly IUserEmailStore<Author> _emailStore;


    public ApiController(
        ICheepService cheepService,
        IAuthorRepository authorRepository,
        ICheepRepository cheepRepository,
        UserManager<Author> userManager,
        IUserStore<Author> userStore)
    {
        _cheepService = cheepService;
        _authorRepository = authorRepository;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
    }


    private const string LatestCommandIdFilePath = "./latest_processed_sim_action_id.txt";

    //Writes the id of the latest command to a text file
    private void Update_Latest(int latestId = -1)
    {
        try
        {
            using StreamWriter writer = new StreamWriter(LatestCommandIdFilePath, false);
            writer.Write(latestId.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred while updating latest id: " + ex.Message);
        }
    }

    //Returns the id of the latest command read from a text file and defaults to -1
    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        try
        {
            if (System.IO.File.Exists(LatestCommandIdFilePath))
            {
                string fileContent = System.IO.File.ReadAllText(LatestCommandIdFilePath);
                if (!int.TryParse(fileContent, out var latestProcessedCommandId))
                {
                    latestProcessedCommandId = -1;
                }
                return Ok(new { latest = latestProcessedCommandId });
            }
            return Ok(new { latest = -1 });
        }
        catch (Exception ex)
        {
            // Handle exception appropriately, e.g., log it
            Console.WriteLine("Error occurred while getting latest id: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromQuery] int latest, [FromBody] RegisterUserData data)
    {

        Update_Latest(latest);

        var user = CreateUser();

        await _userStore.SetUserNameAsync(user, data.username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, data.email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, data.pwd);

        if (result.Succeeded) return Ok("Register a user");
        return StatusCode(401);
    }


    [HttpGet("msgs")]
    public IActionResult GetMessagesFromPublicTimeline([FromQuery] int latest, [FromQuery] int no)
    {
        Update_Latest(latest);


        switch (no)
        {
            case < 0:
                {
                    return BadRequest("'no' cannot be negative");
                }
            case < 32:
                {
                    var result = _cheepService.GetCheeps(1).Take(no);

                    return Ok(result);
                }
            case > 32:
                {
                    var result = _cheepService.GetCheeps(1).Take(32);
                    for (int i = 2; i < (no - 32) / 32; i++)
                    {
                        result = result.Concat(_cheepService.GetCheeps(i).Take(32));
                    }

                    result = result.Take(no);

                    var response = Ok(result);

                    return Ok(result);

                }
        }

        return BadRequest("Parameter 'no' is invalid");
    }



    [HttpGet("msgs/{username}")]
    public IActionResult GetUserMessages([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no)
    {
        Update_Latest(latest);

        //TODO Check if user is authorized
        bool isAuthorized = true;
        if (!isAuthorized)
        {
            return Unauthorized("You are not authorized to view this user's cheeps");
        }

        //TODO check if the requested user exists

        //TODO Return result
        try
        {
            switch (no)
            {
                case < 0:
                    {
                        return BadRequest("'no' cannot be negative");
                    }
                case < 32:
                    {
                        var result = _cheepService.GetCheepsFromAuthor(username, 1).Take(no);
                        if (result.Count() == 0)
                        {
                            return NotFound("This User does not have any Cheeps");
                        }
                        return Ok(result);
                    }
                case > 32:
                    {
                        var result = _cheepService.GetCheeps(1).Take(32);
                        for (int i = 2; i < (no - 32) / 32; i++)
                        {
                            result = result.Concat(_cheepService.GetCheepsFromAuthor(username, i).Take(32));
                        }

                        result = result.Take(no);

                        var response = Ok(result);

                        return Ok(result);

                    }
            }

            return BadRequest("Parameter 'no' is invalid");
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }

        return BadRequest();
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessage([FromRoute] string username, [FromQuery] int latest, [FromBody] MsgsData Msgsdata)
    {
        try
        {   
            
            Author user =_authorRepository.GetAuthorByName(username);
            
            CreateCheep cheep = new CreateCheep(user, Msgsdata.content);
            
            var result =_cheepRepository.AddCreateCheep(cheep);
            
            Update_Latest(latest);
            return Ok(result);
            
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
       
        
    }

    
    [HttpGet("fllws/{username}")]
    public IActionResult GetUserFollowers([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        Update_Latest(latest);

        //TODO Add check of author, check if request is authorized
        var authorFollowers = _authorRepository.GetFollowersById(
            _authorRepository.GetAuthorByName(username).Id);
        var output = new List<String>();
        for (int i = 0; i < authorFollowers.Count; i++)
        {
            if (i > no - 1) break;
            output.Add(authorFollowers.ElementAt(i).UserName);
        }

        return Ok(output.GetRange(0, no));
    }

    [HttpPost("fllws/{username}")]
    public IActionResult FollowUser([FromRoute] string username, [FromQuery] int latest, [FromBody] FollowData followData)
    {
        Update_Latest(latest);

        // Check if at least one action is specified
        if (string.IsNullOrEmpty(followData.follow) && string.IsNullOrEmpty(followData.unfollow))
        {
            return BadRequest("Only one of 'follow' xor 'unfollow' should be provided.");
        }
        if (!string.IsNullOrEmpty(followData.follow) && !string.IsNullOrEmpty(followData.unfollow))
        {
            return BadRequest("Either 'follow' xor 'unfollow' should be provided.");
        }

        try
        {
            if (!string.IsNullOrEmpty(followData.follow))
            {
                var followed = _authorRepository.GetAuthorByName(username);
                var follower = _authorRepository.GetAuthorByName(followData.follow);
                _authorRepository.AddFollow(follower, followed);
                return Ok($"{followData.follow} now follows {username}");
            }

            if (!string.IsNullOrEmpty(followData.unfollow))
            {
                var followed = _authorRepository.GetAuthorByName(username);
                var follower = _authorRepository.GetAuthorByName(followData.unfollow);
                _authorRepository.RemoveFollow(follower, followed);
                return Ok($"{followData.unfollow} no longer follows {username}");
            }
        }
        catch (Exception)
        {
            return BadRequest("");
        }
        return BadRequest("");
    }




    // ######################## TEMP
    public class MsgsData
    {
        public string content { get; set; }
    }
    public class FollowData
    {
        public string? follow { get; set; }
        public string? unfollow { get; set; }

    }

    public class RegisterUserData
    {
        public string username { get; set; }
        public string email { get; set; }
        public string pwd { get; set; }
    }


    private IUserEmailStore<Author> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<Author>)_userStore;
    }

    private Author CreateUser()
    {
        try
        {
            return Activator.CreateInstance<Author>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of 'Author'. " +
                                                $"Ensure that 'Author' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }


}
