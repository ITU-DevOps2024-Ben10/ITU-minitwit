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
    

    //Returns the id of the latest command read from a text file and defaults to -1
    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }

        
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
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }


        Update_Latest(latest);

        var user = CreateUser();

        await _userStore.SetUserNameAsync(user, data.username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, data.email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, data.pwd);

        if (result.Succeeded) return Ok($"Successfully registered user {data.username}");
        return BadRequest($"Registration failed. User {data.username} likely already exists");
    }


    [HttpGet("msgs")]
    public IActionResult GetMessagesFromPublicTimeline([FromQuery] int latest, [FromQuery] int no)
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }
        
        
        Update_Latest(latest);

        if (no < 0)
        {
            return BadRequest("Parameter 'no' is invalid");
        }


        try
        {
            var result = _cheepRepository.GetCheepsByCount(no);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpGet("msgs/{username}")]
    public IActionResult GetUserMessages([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no)
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }
        
        Update_Latest(latest);
        
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
    public async Task<IActionResult> PostMessage([FromRoute] string username, [FromQuery] int latest, [FromBody] MsgsData msgsdata)
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }

        
        try
        {   
            
            Author user =_authorRepository.GetAuthorByName(username);
            
            CreateCheep cheep = new CreateCheep(user.Id, msgsdata.content);
            
            var result = await _cheepRepository.AddCreateCheep(cheep);
            
            Update_Latest(latest);
            return Ok(result);
            
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
       
        
    }

    
    [HttpGet("fllws/{username}")]
    public IActionResult GetUserFollowers([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }

        
        Update_Latest(latest);
        var output = new List<String>();

        try
        {
            var authorFollowers = _authorRepository.GetFollowersById(_authorRepository.GetAuthorByName(username).Id);
            for (int i = 0; i < authorFollowers.Count; i++)
            {
                if (i > no - 1) break;
                output.Add(authorFollowers.ElementAt(i).UserName);
            }

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok(output.Take(no));
    }

    [HttpPost("fllws/{username}")]
    public IActionResult FollowUser([FromRoute] string username, [FromQuery] int latest, [FromBody] FollowData followData)
    {
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return BadRequest("You are not authorized to use this resource");
        }

        
        Update_Latest(latest);

        // Check if exactly one action is specified
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
                var followed = _authorRepository.GetAuthorByName(followData.follow);
                var follower = _authorRepository.GetAuthorByName(username);
                _authorRepository.AddFollow(follower.Id, followed.Id);
                return Ok($"{follower.UserName} now follows {followed.UserName}");
            }

            if (!string.IsNullOrEmpty(followData.unfollow))
            {
                var followed = _authorRepository.GetAuthorByName(followData.unfollow);
                var follower = _authorRepository.GetAuthorByName(username);
                _authorRepository.RemoveFollow(follower.Id, followed.Id);
                return Ok($"{follower.UserName} no longer follows {followed.UserName}");
            }
        }
        catch (Exception)
        {
            return BadRequest("");
        }
        return BadRequest("");
    }




    // Data containers
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

    
    // Helper methods

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
    
    
    public bool NotReqFromSimulator(HttpRequest request)
    {
        string fromSimulator = request.Headers["Authorization"];
        if (fromSimulator != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh")
        {
            return true;
        }
        return false;
    }
    
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

}
