using Microsoft.Playwright;

namespace AutoAIAgent.Services;

public class LinkedInPoster
{
    private readonly string _email;
    private readonly string _password;

    public LinkedInPoster(string email, string password)
    {
        _email = email;
        _password = password;
    }

    public async Task PostAsync(string text, string? imagePath = null)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 50
        });

        var context = await browser.NewContextAsync(new()
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });

        var page = await context.NewPageAsync();

        // ================= LOGIN =================
        await page.GotoAsync("https://www.linkedin.com/login");
        await page.FillAsync("#username", _email);
        await page.FillAsync("#password", _password);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/feed/**", new() { Timeout = 60000 });

        // ================= START POST =================
        await page.ClickAsync("button:has-text('Start a post')");
        await page.WaitForSelectorAsync("div[role='textbox']", new() { Timeout = 15000 });

        // Type text
        await page.FillAsync("div[role='textbox']", "");
        await page.Keyboard.TypeAsync(text, new KeyboardTypeOptions { Delay = 20 });

        // ================= IMAGE UPLOAD =================
        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
        {
            await page.ClickAsync("button[aria-label='Add media']");

            var fileInput = await page.WaitForSelectorAsync("input[type='file']", new() { Timeout = 10000 });
            await fileInput.SetInputFilesAsync(imagePath);

            // Wait for "Next" to appear and click
            var nextButton = await page.WaitForSelectorAsync("button:has-text('Next')", new() { Timeout = 15000 });
            await nextButton.ClickAsync();
        }

        // ================= AUDIENCE =================
        var audienceButton = await page.WaitForSelectorAsync("button[aria-haspopup='dialog']", new() { Timeout = 10000 });
        await audienceButton.ClickAsync();

        var anyoneOption = await page.WaitForSelectorAsync("#ANYONE", new() { Timeout = 10000 });
        await anyoneOption.ClickAsync();

        var doneButton = await page.WaitForSelectorAsync("button:has-text('Done')", new() { Timeout = 10000 });
        await doneButton.ClickAsync();

        // ================= POST =================
        var postButton = await page.WaitForSelectorAsync("button:has-text('Post')", new() { Timeout = 15000 });
        await postButton.ClickAsync();

        // Wait for confirmation / feed reload
        await page.WaitForTimeoutAsync(5000);
    }
}

//public class LinkedInPoster
//{
//    private readonly string _email;
//    private readonly string _password;

//    public LinkedInPoster(string email, string password)
//    {
//        _email = email;
//        _password = password;
//    }

//    public async Task PostAsync(string text, string? imagePath = null)
//    {
//        using var playwright = await Playwright.CreateAsync();

//        await using var browser = await playwright.Chromium.LaunchAsync(
//            new BrowserTypeLaunchOptions
//            {
//                Headless = false,
//                SlowMo = 40
//            });

//        var context = await browser.NewContextAsync(new()
//        {
//            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
//        });

//        var page = await context.NewPageAsync();

//        // ================= LOGIN =================
//        await page.GotoAsync("https://www.linkedin.com/login");
//        await page.FillAsync("#username", _email);
//        await page.FillAsync("#password", _password);
//        await page.ClickAsync("button[type='submit']");

//        await page.WaitForURLAsync("**/feed/**", new() { Timeout = 60000 });

//        // ================= CREATE POST =================
//        await page.ClickAsync("button:has-text('Start a post')");
//        await page.WaitForSelectorAsync("div[role='textbox']");
//        await page.FillAsync("div[role='textbox']", text);

//        // ================= IMAGE =================
//        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
//        {
//            await page.ClickAsync("button[aria-label='Add media']");
//            var input = await page.WaitForSelectorAsync("input[type='file']");
//            await input.SetInputFilesAsync(imagePath);

//            await page.ClickAsync("button:has-text('Next')");
//        }

//        // ================= AUDIENCE =================
//        await page.ClickAsync("button[aria-haspopup='dialog']");
//        await page.ClickAsync("#ANYONE");
//        await page.ClickAsync("button:has-text('Done')");

//        // ================= POST =================
//        await page.ClickAsync("button:has-text('Post')");
//        await page.WaitForTimeoutAsync(5000);
//    }
//}
