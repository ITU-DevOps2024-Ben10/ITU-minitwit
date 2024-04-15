using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Minitwit.End2EndTest.UITest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class UiNotLoggedIn : PageTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
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
    public async Task CheckPublicTimeline()
    {
            await _page.GotoAsync(_publicUrl);
            
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
        await _page.GotoAsync(_publicUrl);

        await _page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync(_loginUrl);
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Public timeline" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync(_publicUrl);
        
        await _page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(_page).ToHaveURLAsync(_registerUrl);
        
    }
    [Test]
    public async Task CheckRegister()
    {
        await _page.GotoAsync(_registerUrl);
        
        await Expect( _page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
        
        await Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Register", Exact = true })).ToBeVisibleAsync();
        await Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Create a new account." })).ToBeVisibleAsync();
        await Expect (_page.GetByText("Username")).ToBeVisibleAsync();
        await Expect (_page.Locator("#registerForm div").Filter(new() { HasText = "Email" })).ToBeVisibleAsync();
        await Expect (_page.GetByText("Password", new() { Exact = true })).ToBeVisibleAsync();
        await Expect (_page.GetByText("Confirm Password")).ToBeVisibleAsync();
        
    }

    [Test]
    public async Task CheckLogin()
    {
        await _page.GotoAsync(_loginUrl);
        
        await Expect (_page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Exact = true })).ToBeVisibleAsync();
        await Expect (_page.GetByRole(AriaRole.Heading, new() { Name = "Use a local account to log in." })).ToBeVisibleAsync();
        await Expect (_page.GetByText("Username")).ToBeVisibleAsync();
        await Expect (_page.GetByText("Password", new() { Exact = true })).ToBeVisibleAsync(); 
    }
    

    [TearDown]
    public async Task TearDownAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}