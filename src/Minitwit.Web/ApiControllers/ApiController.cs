using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;
/*
 * TODO REMOVE THIS COMMENT WHEN THE API IS DONE
 *
 * This is a rough draft of our API, logic its not perfect and it is not complete.
 * For now we're just experimenting with the API and how it should be structured.
 */
namespace Minitwit.Web.ApiControllers;
    
    //Isn't needed as the endpoints we need to expose doesn't clash with any endpoints we already use
    //[Route("api")]
    [ApiController]
    public class ApiController : ControllerBase

    {
        private readonly ICheepService _cheepService;
        private readonly IAuthorRepository _authorRepository;
        public ApiController(ICheepService cheepService, IAuthorRepository authorRepository)
        {
            _cheepService = cheepService;
            _authorRepository = authorRepository;
        }


        private const string LatestCommandIdFilePath = "./latest_processed_sim_action_id.txt";

        //Writes the id of the latest command to a text file
        private void Update_Latest(int latestId)
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
        public IActionResult RegisterUser([FromQuery] int latest)
        {
            Update_Latest(latest);
           
            return Ok("Register a user");
        }

        
        [HttpGet("msgs")]
        public IActionResult GetMessagesFromPublicTimeline([FromQuery] int latest)
        {
            Update_Latest(latest);
            
            //TODO Validate input and generally finalize the handling of this endpoint
            var numberOfCheeps = IntegerType.FromString(Request.Query["no"]) ;
            
            switch (numberOfCheeps)
            {
                case < 32:
                {
                    var result = _cheepService.GetCheeps(1).Take(numberOfCheeps);
                    var response = Ok(result);

                    Update_Latest(latest);

                    return response;
                }
                case > 32:
                {
                    var result = _cheepService.GetCheeps(1).Take(32);
                    for (int i = 2; i < (numberOfCheeps-32)/32; i++)
                    {
                        result = result.Concat(_cheepService.GetCheeps(i).Take(32));
                    }
                    
                    result = result.Take(numberOfCheeps);
                    
                    var response = Ok(result);
                    Update_Latest(latest);
                    
                    return Ok(result);
                    
                }
            }
            
            return BadRequest("Parameter 'no' is invalid");
        }
        

       
        [HttpGet("msgs/{username}")]
        public IActionResult GetUserMessages([FromRoute] string username, [FromQuery] int latest)
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
            
            var result = _cheepService.GetCheepsFromAuthor(username, 1);
            if (result.Count == 0)
            {
                return NotFound("This User does not have any Cheeps");
            }
            return Ok(result);
        }

        
        [HttpGet("fllws/{username}")]
        public IActionResult GetUserFollowers([FromRoute] string username, [FromQuery] int latest)
        {
            Update_Latest(latest);

            //TODO Add check of author, check if request is authorized
            var authorFollowers = _authorRepository.GetFollowersById(
                                                        _authorRepository.GetAuthorByName(username).Id);
            
            return Ok(authorFollowers);
            

        }
        
        [HttpPost("fllws/{username}")]
        public IActionResult FollowUser([FromRoute] string username, [FromQuery] int latest)
        {
            Update_Latest(latest);

            return Ok(""); 
            
        }
    }