using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minitwit.Infrastructure;
using Minitwit.Web;
using Minitwit.Web.ApiControllers;
using Test_Utilities;

namespace Minitwit.WebTests.Api;

public class ApiControllerTests
{
    private ApiController _apiController;
    private MinitwitDbContext _db;

    public ApiControllerTests()
    {
        _db = SqliteInMemoryBuilder.GetContext();
        
        _apiController = new ApiController(
            new MinitwitService()
            )
    }
    
    
    [Fact]
    public void GetLatest_WithoutCorrectAuthorization_ReturnsForbidden()
    {
        // Arrange
        var controller = new ApiController(/* dependencies */);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() // Mock HttpContext
        };
        // Simulate missing or incorrect Authorization header
        controller.HttpContext.Request.Headers["Authorization"] = "IncorrectToken";

        // Act
        var result = controller.GetLatest();

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }
}