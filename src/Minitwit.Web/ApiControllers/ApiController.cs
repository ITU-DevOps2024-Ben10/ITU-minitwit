using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;
using MySqlX.XDevAPI.Common;
using System.Text;
using static Minitwit.Web.ApiControllers.ApiController;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    
    private static readonly Meter MyMeter = new Meter("minitwit-api-requests-total");
    private static readonly Counter<long> ApiRequestCounter = MyMeter.CreateCounter<long>(
        "api_requests_total",
        "Total number of API requests"
    );


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
    private const string latestLogFilePath = "./LogLatestGet.txt";
    private const string registerLogFilePath = "./LogRegisterPost.txt";
    private const string msgsGetLogFilePath = "./LogMsgsGet.txt";
    private const string msgsPrivateGetLogFilePath = "./LogMsgsPrivateGet.txt";
    private const string msgsPostLogFilePath = "./LogMsgsPost.txt";
    private const string fllwsGetLogFilePath = "./LogFllwsGet.txt";
    private const string fllwsPostLogFilePath = "./LogFllwsPost.txt";


    //Returns the id of the latest command read from a text file and defaults to -1
    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        ApiRequestCounter.Add(1);
        
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource!");
        }

        
        try
        {
            if (!System.IO.File.Exists(LatestCommandIdFilePath)) return Ok(new { latest = -1 });
            string fileContent = System.IO.File.ReadAllText(LatestCommandIdFilePath);
            if (!int.TryParse(fileContent, out var latestProcessedCommandId))
            {
                latestProcessedCommandId = -1;
            }
            return Ok(new { latest = latestProcessedCommandId });
        }
        catch (Exception ex)
        {
            LogRequest("{}", $"{{{ex.Message}}}", latestLogFilePath);

            // Handle exception appropriately, e.g., log it
            Console.WriteLine("Error occurred while getting latest id: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromQuery] int latest, [FromBody] RegisterUserData data)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }


        Update_Latest(latest);

        var user = CreateUser();

        await _userStore.SetUserNameAsync(user, data.username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, data.email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, data.pwd);


        if (result.Succeeded) return StatusCode(204,"");

        LogRequest(data.ToString(), StringifyIdentityResultErrors(result), registerLogFilePath);

        return BadRequest($"{result.Errors.ToList()}");
    }


    [HttpGet("msgs")]
    public IActionResult GetMessagesFromPublicTimeline([FromQuery] int latest, [FromQuery] int no = 100)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }
        
        
        Update_Latest(latest);

        if (no < 0)
        {
            no = 100;
        }


        try
        {
            var cheeps = _cheepRepository.GetCheepsByCount(no).ToList();
            var users = _authorRepository.GetAllAuthors().Where(a => cheeps.Any(c => a.Id == c.AuthorId)).ToList();

            List<CheepViewModelApi> lst = new();

            foreach (var cheep in cheeps)
            {
                lst.Add(new CheepViewModelApi(users.FirstOrDefault(a => a.Id == cheep.AuthorId)!.UserName,
                    cheep.Text, cheep.TimeStamp));
            }
            
            return Ok(lst);
        }
        catch (Exception ex)
        {
            LogRequest($"{{Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", msgsGetLogFilePath);
            return NotFound();
        }
    }



    [HttpGet("msgs/{username}")]
    public IActionResult GetUserMessages([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }
        
        Update_Latest(latest);
        
        if (no < 0)
        {
            no = 100;
        }
        
        try
        {
            Guid authorId = _authorRepository.GetAuthorByName(username).Id;
            var cheeps = _cheepRepository.GetCheepsFromAuthorByCount(authorId, no);

            List<CheepViewModelApi> formattedCheeps = new();

            foreach (var cheep in cheeps)
            {
                formattedCheeps.Add(new CheepViewModelApi(username, cheep.Text, cheep.TimeStamp));
            }
            
            return Ok(formattedCheeps);

        }
        catch (Exception ex)
        {
            LogRequest($"{{User = {username}, Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", msgsPrivateGetLogFilePath);
            return NotFound();
        }
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessage([FromRoute] string username, [FromQuery] int latest, [FromBody] MsgsData msgsdata)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }

        
        try
        {   
            
            Author user =_authorRepository.GetAuthorByName(username);
            
            CreateCheep cheep = new CreateCheep(user.Id, msgsdata.content);
            
            var result = await _cheepRepository.AddCreateCheep(cheep);
            
            Update_Latest(latest);
            return StatusCode(204,"");
            
        }
        catch (Exception ex)
        {
            LogRequest(msgsdata.ToString(), $"{{{ex.Message}}}", msgsPostLogFilePath);

            return NotFound();
        }
       
        
    }

    
    [HttpGet("fllws/{username}")]
    public IActionResult GetUserFollowers([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }
        
        Update_Latest(latest);
        var output = new List<string>();

        try
        {
            var authorFollowers = _authorRepository.GetFollowersById(_authorRepository.GetAuthorByName(username).Id);
            for (int i = 0; i < authorFollowers.Count; i++)
            {
                if (i > no - 1) break;
                output.Add(authorFollowers.ElementAt(i).UserName);
            }

        }
        catch (Exception ex)
        {
            LogRequest($"{{User = {username}, Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", fllwsGetLogFilePath);
            return NotFound();
        }

        return Ok(new { follows = output.Take(no) });
        
    }

    [HttpPost("fllws/{username}")]
    public IActionResult FollowUser([FromRoute] string username, [FromQuery] int latest, [FromBody] FollowData followData)
    {
        ApiRequestCounter.Add(1);
        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
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
                return StatusCode(204, "");
            }

            if (!string.IsNullOrEmpty(followData.unfollow))
            {
                var followed = _authorRepository.GetAuthorByName(followData.unfollow);
                var follower = _authorRepository.GetAuthorByName(username);
                _authorRepository.RemoveFollow(follower.Id, followed.Id);
                return StatusCode(204, "");
            }
        }
        catch (Exception ex)
        {
            LogRequest($"User = {username}. Request body: {followData}", $"{{{ex.Message}}}", fllwsPostLogFilePath);
            return NotFound();
        }
        return NotFound();
    }




    // Data containers

    private interface IData
    {
        public string GetData();
    }

    public class MsgsData : IData
    {
        public string content { get; set; }

        public string GetData() { return ToString(); }
        public override string ToString() { return $"{{content: {content}}}"; }

    }
    public class FollowData : IData
    {
        public string? follow { get; set; }
        public string? unfollow { get; set; }

        public string GetData() { return ToString(); }
        public override string ToString() { return $"{{follow: {follow}, unfollow: {unfollow}}}"; }

    }

    public class RegisterUserData : IData
    {
        public string username { get; set; }
        public string email { get; set; }
        public string pwd { get; set; }

        public string GetData() { return ToString(); }  
        public override string ToString() { return $"{{Username: {username}, Email: {email}, Password: {pwd}}}"; }
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
        return request.Headers.Authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
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

    private void LogRequest(string data, string errors, string logFilePath)
    {
        // Stringify header
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("{");
        foreach (var header in Request.Headers.ToList())
        {
            stringBuilder.Append($"{header.Key}: {header.Value}, ");
        }
        stringBuilder.Append("}");
        string headers = stringBuilder.ToString();

        // format everything
        string logtext = $"{headers}\n{data}\n{errors}\n\n";

        using StreamWriter writer = new StreamWriter(logFilePath, true);
        writer.Write(logtext);
    }


    private string StringifyIdentityResultErrors(IdentityResult result)
    {
        // Stringify Errors
        StringBuilder stringBuilderError = new StringBuilder();
        stringBuilderError.Append("{");
        foreach (var error in result.Errors.ToList())
        {
            stringBuilderError.Append($"\"{error.Description}\", ");
        }
        stringBuilderError.Append("}");
        return stringBuilderError.ToString();
    }

}
