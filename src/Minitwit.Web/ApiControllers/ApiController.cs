using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Web;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;

namespace Minitwit.Web.ApiControllers;
    
    //Isn't needed as the endpoints we need to expose doesn't clash with any endpoints we already use
    //[Route("api")]
    [ApiController]
    public class ApiController : ControllerBase

    {
        private readonly ICheepService _cheepService;
        private ObjectResult? latestResponse;
        public ApiController(ICheepService cheepService)
        {
            
            _cheepService = cheepService; 
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

            if (numberOfCheeps < 32)
            {
                var result = _cheepService.GetCheeps(1).Take(numberOfCheeps + 1);
                var response = Ok(result);

                latestResponse = response;

                return response;
            }

            return BadRequest("Parameter 'no' is invalid");
        }
        

       
        [HttpGet("msgs/{username}")]
        public IActionResult GetUserMessages([FromQuery] string username)
        {
            //TODO Check if user is authorized
            
            //TODO check if the requested user exists
            
            //TODO Return result
            
            
            
            return Ok("User message endpoint");
        }

        
        [HttpGet("fllws/{username}")]
        public IActionResult GetUserFollowers([FromQuery] string username)
        {
            return Ok("See a users followers"); 
            
        }
        
        [HttpPost("fllws/{username}")]
        public IActionResult FollowerUser([FromQuery] string username)
        {
            return Ok("Follow a user"); 
            
        }
    }
