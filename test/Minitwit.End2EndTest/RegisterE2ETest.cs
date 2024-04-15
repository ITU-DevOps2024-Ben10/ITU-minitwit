using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Minitwit.End2EndTest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegisterE2ETest : PageTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    
    private readonly Dictionary<string, string> _user =
        new() {{"username", "TestUser123"}, {"email", "TestEmail@test.test"}, {"password", "TestPassword"}, {"CheepText", "Hello fellow students! 4283"}};
    private readonly string _publicUrl = "http://localhost:8080/";
    private readonly string _registerUrl = "http://localhost:8080/Identity/Account/Register";
    private readonly string _loginUrl = "http://localhost:8080/Identity/Account/Login";
    
    [SetUp]
    public async Task SetUpAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 2000
        });
        _page = await _browser.NewPageAsync();
        

    }

    [Test]
    public async Task RegisterUserViaGuiTestWithCredentials()
    {
         await _page.GotoAsync(_registerUrl);
         
         await _page.GetByPlaceholder("username").FillAsync(_user["username"]);
        
         await _page.GetByPlaceholder("name@example.com").FillAsync(_user["email"]);
         
         await _page.FillAsync("input[id='Input_Password']", _user["password"]);
         await _page.FillAsync("input[id=Input_ConfirmPassword]", _user["password"]);
         
         await _page.ClickAsync("button[type=submit]");
         
         
         await _page .WaitForURLAsync(_publicUrl);
         await Expect(_page).ToHaveURLAsync(_publicUrl);
    }
    [Test]
    public async Task MakeCheepUsingRegisteredUser()
    {
        await _page.GotoAsync(_publicUrl);
        
        await Expect(_page.GetByRole(AriaRole.Heading, new () 
            {Name = "What's on your mind " + _user["username"] + "?"})).ToBeVisibleAsync();
        
        await Expect(Page.GetByText("Write your text here")).ToBeVisibleAsync();
        
        await Page.GetByLabel("Write your text here").FillAsync(_user["CheepText"]);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cheep" }).ClickAsync();
        
        await Expect(_page).ToHaveURLAsync(_publicUrl + _user["username"]);
        
        await Expect(_page.GetByRole(AriaRole.Heading, new () {Name = "TestUser123's Timeline"}))
            .ToBeVisibleAsync();
        await Expect(_page.GetByText(_user["CheepText"])).ToBeVisibleAsync();

    }

    [Test]
    public async Task CheepShouldBeVisibleInPublic()
    {
        await _page.GotoAsync(_publicUrl);
        
        await Expect(_page.GetByText(_user["CheepText"])).ToBeVisibleAsync();
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
//pwsh bin/Debug/net8.0/playwright.ps1 codegen 
