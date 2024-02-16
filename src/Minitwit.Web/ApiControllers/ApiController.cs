using Microsoft.AspNetCore.Mvc;

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
        
        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            
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
