using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using Minitwit.Web;
using Minitwit.Web.ApiControllers;
using Test_Utilities;

//Mocking framework
using Moq;

namespace Minitwit.WebTests;

public class ApiControllerTests
{
    private readonly ApiController _apiController;
    
    //Mocking dependencies
    private readonly Mock<ICheepService> _mockCheepService = new ();
    private readonly Mock<IAuthorRepository> _mockAuthorRepository = new ();
    private readonly Mock<ICheepRepository> _mockCheepRepository = new ();
    private readonly Mock<IUserStore<Author>> _mockUserStore = new();
    private readonly Mock<UserManager<Author>> _mockUserManager = new(new Mock<IUserStore<Author>>().Object, null, null, null, null, null, null, null, null);

    
    public ApiControllerTests()
    {
        _mockUserStore.As<IUserEmailStore<Author>>()
            .Setup(store => store.SetEmailAsync(It.IsAny<Author>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUserManager.Setup(store => store.SupportsUserEmail).Returns(true);
        
        _apiController = new ApiController(
            _mockCheepService.Object,
            _mockAuthorRepository.Object,
            _mockCheepRepository.Object,
            _mockUserManager.Object,
            _mockUserStore.Object
        );
        
    }

    [Fact]
    public void get_msgs_should_return_ok()
    {
        //Arrange
        int count = 2;
        
        ICollection<Cheep> cheeps = new List<Cheep>();
        cheeps.Add(new Cheep { Text = "Hello" });
        cheeps.Add(new Cheep { Text = "Hello1" });

        _mockCheepRepository.Setup(repo => repo.GetCheepsByCount(count))
            .Returns(cheeps);
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = ;
    
        //Act
        var result = _apiController.GetMessagesFromPublicTimeline(1, count);
        
        //Assert
        var statusCodeResult = result as StatusCodeResult;
        
        Assert.Equal(200, statusCodeResult.StatusCode);


    }
    

    
}