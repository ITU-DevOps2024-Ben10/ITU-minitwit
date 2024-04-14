using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Minitwit.End2EndTest.UITest;

public class UiNotLoggedIn : PageTest
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
            //Headless = false,
            //SlowMo = 2000
        });
        _page = await _browser.NewPageAsync();
        
    }

    [Test]
    public async Task CheckPublicTimeline()
    {
            await _page.GotoAsync("http://localhost:8080/");
            
            //Check Navbar
            await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
            await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
            await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
            
            //Check Public Timeline 
            await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
            //Check the the page number can be reached
            await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "[1]" })).ToBeVisibleAsync();
            
         
    }

    [Test]
    public async Task CheckNavBarReRouting()
    {
        await _page.GotoAsync("http://localhost:8080/");

        await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync("http://localhost:8080/Identity/Account/Login");
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Public timeline" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync("http://localhost:8080/");
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync("http://localhost:8080/Identity/Account/Register");
        
    }
    [Test]
    public async Task CheckRegister()
    {
        await _page.GotoAsync("http://localhost:8080/Identity/Account/Register");
        
        Expect( _page.GetByRole(AriaRole.Link, new() { Name = "Register" }));
        Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Register", Exact = true }));
        Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Create a new account." }));
        Expect (_page.GetByText("Username"));
        Expect (_page.Locator("#registerForm div").Filter(new() { HasText = "Email" }));
        Expect (_page.GetByText("Password", new() { Exact = true }));
        Expect (_page.GetByText("Confirm Password"));
        
    }

    [Test]
    public async Task CheckLogin()
    {
        await _page.GotoAsync("http://localhost:8080/Identity/Account/Login");
        
        Expect (_page.GetByRole(AriaRole.Link, new() { Name = "Login" }));
        Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true }));
        Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Use a local account to log in." }));
        Expect (_page.GetByText("Username"));
        Expect (_page.GetByText("Password", new() { Exact = true })); 
    }
    

    [TearDown]
    public async Task TearDownAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}