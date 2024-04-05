using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web.Models;
using System.Text;

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
    private readonly IAuthorRepository _authorRepository;
    private readonly ICheepRepository _cheepRepository;
    private readonly UserManager<Author> _userManager;
    private readonly IUserStore<Author> _userStore;
    private readonly IUserEmailStore<Author> _emailStore;


    public ApiController(
        IAuthorRepository authorRepository,
        ICheepRepository cheepRepository,
        UserManager<Author> userManager,
        IUserStore<Author> userStore)
    {
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
    public async Task<IActionResult> GetLatest()
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource!");
        }


        try
        {
            if (!System.IO.File.Exists(LatestCommandIdFilePath)) return Ok(new { latest = -1 });
            string fileContent = await System.IO.File.ReadAllTextAsync(LatestCommandIdFilePath);
            if (!int.TryParse(fileContent, out var latestProcessedCommandId))
            {
                latestProcessedCommandId = -1;
            }
            return Ok(new { latest = latestProcessedCommandId });
        }
        catch (Exception ex)
        {
            await LogRequest("{}", $"{{{ex.Message}}}", latestLogFilePath);

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
            return StatusCode(403, "You are not authorized to use this resource");
        }

        await Update_Latest(latest);

        var result = await CreateUser(data.username, data.email, data.pwd);


        if (result.Succeeded) return StatusCode(204, "");

        await LogRequest(data.ToString(), StringifyIdentityResultErrors(result), registerLogFilePath);

        return BadRequest($"{result.Errors.ToList()}");
    }


    [HttpGet("msgs")]
    public async Task<IActionResult> GetMessagesFromPublicTimeline([FromQuery] int latest, [FromQuery] int no = 100)
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }


        await Update_Latest(latest);

        if (no < 0)
        {
            no = 100;
        }


        try
        {

            var cheeps = await _cheepRepository.GetCheepsByCountAsync(no);
            var users = _authorRepository.GetAllAuthorsAsync().Result.Where(a => cheeps.Any(c => a.Id == c.AuthorId)).ToList();

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
            await LogRequest($"{{Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", msgsGetLogFilePath);
            return NotFound();
        }
    }



    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetUserMessages([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }

        await Update_Latest(latest);

        if (no < 0)
        {
            no = 100;
        }

        try
        {

            // Create user if it doesn't exist
            if (_authorRepository.GetAuthorByNameAsync(username) == null)
            {
                await CreateUser(username, $"{username}@user.com", "password");
            }

            Author author = await _authorRepository.GetAuthorByNameAsync(username);
            Guid authorId = author.Id;
            ICollection<Cheep> cheeps = await _cheepRepository.GetCheepsFromAuthorByCountAsync(authorId, no);
            List<CheepViewModelApi> formattedCheeps = new();

            foreach (var cheep in cheeps)
            {
                formattedCheeps.Add(new CheepViewModelApi(username, cheep.Text, cheep.TimeStamp));
            }

            return Ok(formattedCheeps);

        }
        catch (Exception ex)
        {
            await LogRequest($"{{User = {username}, Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", msgsPrivateGetLogFilePath);
            return NotFound();
        }
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessage([FromRoute] string username, [FromQuery] int latest, [FromBody] MsgsData msgsdata)
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }


        try
        {
            // Create user if it doesn't exist
            if (_authorRepository.GetAuthorByNameAsync(username) == null)
            {
                await CreateUser(username, $"{username}@user.com", "password");
            }

            Author user = await _authorRepository.GetAuthorByNameAsync(username);

            CreateCheep cheep = new CreateCheep(user.Id, msgsdata.content);

            var result = await _cheepRepository.AddCreateCheepAsync(cheep);

            await Update_Latest(latest);
            return StatusCode(204, "");

        }
        catch (Exception ex)
        {
            await LogRequest(msgsdata.ToString(), $"{{{ex.Message}}}", msgsPostLogFilePath);

            return NotFound();
        }


    }


    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetUserFollowers([FromRoute] string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }

        await Update_Latest(latest);
        var output = new List<string>();

        try
        {

            // Create user if it doesn't exist
            if (_authorRepository.GetAuthorByNameAsync(username) == null)
            {
                await CreateUser(username, $"{username}@user.com", "password");
            }

            Author author = await _authorRepository.GetAuthorByNameAsync(username);
            var authorFollowers = await _authorRepository.GetFollowersByIdAsync(author.Id);
            for (int i = 0; i < authorFollowers.Count; i++)
            {
                if (i > no - 1) break;
                output.Add(authorFollowers.ElementAt(i).UserName);
            }

        }
        catch (NullReferenceException ex)
        {
            await SimpleLogRequest($"{{User = {username}, Latest = {latest}, No = {no}}}", $"{{{ex.Message}}}", fllwsGetLogFilePath);
            return NotFound();
        }
        catch (Exception ex)
        {
            await LogRequest($"{{User = {username}, Latest = {latest}, No = {no}}}", $"{{{ex.StackTrace}}}", fllwsGetLogFilePath);
            return NotFound();
        }

        return Ok(new { follows = output.Take(no) });

    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> FollowUser([FromRoute] string username, [FromQuery] int latest, [FromBody] FollowData followData)
    {

        // Checks authorization
        if (NotReqFromSimulator(Request))
        {
            return StatusCode(403, "You are not authorized to use this resource");
        }


        await Update_Latest(latest);

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
            // Create user if it doesn't exist
            if (_authorRepository.GetAuthorByNameAsync(username) == null)
            {
                await CreateUser(username, $"{username}@user.com", "password");
            }

            if (!string.IsNullOrEmpty(followData.follow))
            {
                // Create user if it doesn't exist
                if (await _authorRepository.GetAuthorByNameAsync(followData.follow) == null)
                {
                    await CreateUser(followData.follow, $"{followData.follow}@user.com", "password");
                }
                var followed = await _authorRepository.GetAuthorByNameAsync(followData.follow);
                var follower = await _authorRepository.GetAuthorByNameAsync(username);
                await _authorRepository.AddFollowAsync(follower.Id, followed.Id);
                return StatusCode(204, "");
            }

            if (!string.IsNullOrEmpty(followData.unfollow))
            {
                // Create user if it doesn't exist
                if (_authorRepository.GetAuthorByNameAsync(followData.unfollow) == null)
                {
                    await CreateUser(followData.unfollow, $"{followData.unfollow}@user.com", "password");
                }
                var followed = await _authorRepository.GetAuthorByNameAsync(followData.unfollow);
                var follower = await _authorRepository.GetAuthorByNameAsync(username);
                await _authorRepository.RemoveFollowAsync(follower.Id, followed.Id);
                return StatusCode(204, "");
            }
        }
        catch (NullReferenceException ex)
        {
            await SimpleLogRequest($"User = {username}. Request body: {followData}", $"{{{ex.Message}}}", fllwsPostLogFilePath);
            return NotFound();
        }
        catch (Exception e)
        {
            await LogRequest($"User = {username}. Request body: {followData}", $"{{{e.StackTrace}}}", fllwsPostLogFilePath);
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

    private async Task<IdentityResult> CreateUser(string username, string email, string password)
    {
        var user = CreateUser();

        await _userStore.SetUserNameAsync(user, username, CancellationToken.None);
        await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
        return await _userManager.CreateAsync(user, password);
    }

    public bool NotReqFromSimulator(HttpRequest request)
    {
        return request.Headers.Authorization != "Basic c2ltdWxhdG9yOnN1cGVyX3NhZmUh";
    }

    //Writes the id of the latest command to a text file
    private async Task Update_Latest(int latestId = -1)
    {
        try
        {
            await using (StreamWriter writer = new StreamWriter(LatestCommandIdFilePath, false))
            {
                await writer.WriteAsync(latestId.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while updating latest id: {ex.Message}");
        }
    }

    private async Task SimpleLogRequest(string data, string errors, string logFilePath)
    {
        // format everything
        string logtext = $"{data}\n{errors}\n\n";

        await using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            await writer.WriteAsync(logtext);
        }
    }

    private async Task LogRequest(string data, string errors, string logFilePath)
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

        await using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            await writer.WriteAsync(logtext);
        }
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
