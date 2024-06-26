using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Minitwit.End2EndTest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RegisterE2ETest : PageTest
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    private static readonly Dictionary<string, string> User =
        new()
        {
            { "username", "TestUser123" },
            { "email", "TestEmail@test.test" },
            { "password", "TestPassword" },
            { "CheepText", "Hello fellow students! 4283" }
        };
    private readonly string _publicUrl = "http://localhost:8080/";
    private readonly string _registerUrl = "http://localhost:8080/Identity/Account/Register";
    private readonly string _aboutmeUrl = $"http://localhost:8080/{User["username"]}/AboutMe";

    [SetUp]
    public async Task SetUpAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                //Headless = false,
                //SlowMo = 1000
            }
        );
        _page = await _browser.NewPageAsync();
    }

    [Test]
    public async Task RegisterUserViaGuiTestWithCredentials()
    {
        await _page!.GotoAsync(_registerUrl);

        await _page.GetByPlaceholder("username").FillAsync(User["username"]);

        await _page.GetByPlaceholder("name@example.com").FillAsync(User["email"]);

        await _page.FillAsync("input[id='Input_Password']", User["password"]);
        await _page.FillAsync("input[id=Input_ConfirmPassword]", User["password"]);

        await _page.ClickAsync("button[type=submit]");

        await _page.WaitForURLAsync(_publicUrl);
        await Expect(_page).ToHaveURLAsync(_publicUrl);
        //}
        //[Test]
        //public async Task MakeCheepUsingRegisteredUser()
        //{
        await _page.GotoAsync(_publicUrl);

        await Expect(
                _page.GetByRole(
                    AriaRole.Heading,
                    new() { Name = "What's on your mind " + User["username"] + "?" }
                )
            )
            .ToBeVisibleAsync();

        await Expect(_page.GetByRole(AriaRole.Textbox, new() { Name = "" })).ToBeVisibleAsync();

        await _page.GetByLabel("Text").FillAsync(User["CheepText"]);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Cheep" }).ClickAsync();

        await Expect(_page).ToHaveURLAsync(_publicUrl + User["username"]);

        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "TestUser123's Timeline" }))
            .ToBeVisibleAsync();
        await Expect(_page.GetByText(User["CheepText"])).ToBeVisibleAsync();

        //}

        //[Test]
        //public async Task CheepShouldBeVisibleInPublic()
        //{
        await _page.GotoAsync(_publicUrl);
        await Expect(_page.GetByText(User["CheepText"])).ToBeVisibleAsync();
        //}

        //[Test]
        //public async Task AboutMePageShouldBeAccessible()
        //{
        await _page.GotoAsync(_aboutmeUrl);
        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Forget Me" }))
            .ToBeVisibleAsync();

        _page.Dialog += async (_, dialog) =>
        {
            Assert.That(dialog.Type, Is.EqualTo("confirm"));
            await dialog.AcceptAsync();
        };
        await _page.GotoAsync(_aboutmeUrl);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Forget Me" }).ClickAsync();

        await Expect(_page).ToHaveURLAsync(_publicUrl);
    }

    [OneTimeTearDown]
    public async Task TearDownAsync()
    {
        await _browser!.CloseAsync();
        _playwright!.Dispose();
    }
}
//pwsh bin/Debug/net8.0/playwright.ps1 codegen {URL TO TEST ON}
