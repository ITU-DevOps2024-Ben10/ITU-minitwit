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
    
    [SetUp]
    public async Task SetUpAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _page = await _browser.NewPageAsync();
        

    }

    [Test]
    public async Task RegisterUserViaGuiTestWithCredentials()
    {
         await _page.GotoAsync("http://localhost:8080/Identity/Account/Register");
         
         await _page.GetByPlaceholder("username").FillAsync("TestUser123");
        
         await _page.GetByPlaceholder("name@example.com").FillAsync("TestEmail@test.test");
         
         await _page.TypeAsync("input[id='Input_Password']", "TestPassword");
         await _page.TypeAsync("input[id=Input_ConfirmPassword]", "TestPassword");
         
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
//pwsh bin/Debug/net8.0/playwright.ps1 codegen 
