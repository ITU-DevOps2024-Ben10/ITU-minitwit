using Microsoft.AspNetCore.Mvc;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;

namespace Minitwit.Web.ApiControllers;

    /*
     * Endpoints that needs to be exposed:
     * "/latest" GET
     * Should return the latest value
     * 
     * "/register" POST
     * err: 400, Suc: 204
     * "/msgs" GET
     * 
     * "/msgs/<username>" GET POST
     * Abort: 404, Suc: 204
     * "/fllws/<username>" GET POST
     */
    
    
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    
    {
        private readonly ICheepRepository _cheepRepository;
        private readonly IAuthorRepository _authorRepository;
        public ApiController( ICheepRepository cheepRepository, IAuthorRepository authorRepository)
        {
            _cheepRepository = cheepRepository;
            _authorRepository = authorRepository;
        }
        
        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            
            //Should return the latest command called from the simulator 
            return Ok();
        }

        
        [HttpPost("register")]
        public IActionResult RegisterUser()
        {
           
            return Ok();
        }

        
        [HttpGet("msgs")]
        public IActionResult GetAllMessagesFromPublicTimeline()
        {
              
            return Ok("All messages");
        }
        

       
        [HttpGet("msgs/{username}")]
        public IActionResult GetUserMessages(string username)
        {
            return Ok();
        }

        
        [HttpGet("fllws/{username}")]
        public IActionResult GetUserFollowers(string username)
        {
            return Ok(); 
            
        }
        
        [HttpPost("fllws/{username}")]
        public IActionResult FollowerUser()
        {
            return Ok(); 
            
        }
    }
