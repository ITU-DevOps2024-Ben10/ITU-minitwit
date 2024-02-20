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
        private ObjectResult? latestResponse;
        public ApiController(ICheepService cheepService, IAuthorRepository authorRepository)
        {
            _cheepService = cheepService;
            _authorRepository = authorRepository;
        }
        
        
        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            //Should return the latest command called from the simulator 
            return Ok(latestResponse);
        }

        
        [HttpPost("register")]
        public IActionResult RegisterUser()
        {
           
            return Ok("Register a user");
        }

        
        [HttpGet("msgs")]
        public IActionResult GetMessagesFromPublicTimeline()
        {
            //TODO Validate input and generally finalize the handling of this endpoint
            var numberOfCheeps = IntegerType.FromString(Request.Query["no"]) ;
            
            switch (numberOfCheeps)
            {
                case < 32:
                {
                    var result = _cheepService.GetCheeps(1).Take(numberOfCheeps);
                    var response = Ok(result);

                    latestResponse = response;

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
                    latestResponse = response;
                    
                    return Ok(result);
                    
                }
            }
            
            return BadRequest("Parameter 'no' is invalid");
        }
        

       
        [HttpGet("msgs/{username}")]
        public IActionResult GetUserMessages([FromQuery] string username)
        {
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
        public IActionResult GetUserFollowers([FromRoute] string username)
        {
            //TODO Add check of author, check if request is authorized
            var queriedAuthor = _authorRepository.GetAuthorByName(username);
            var result = queriedAuthor.Followers;
            
            latestResponse = Ok(result);
            return Ok(result);
            

        }
        
        [HttpPost("fllws/{username}")]
        public IActionResult FollowUser([FromRoute] string username)
        {
            
            return Ok(""); 
            
        }
    }
