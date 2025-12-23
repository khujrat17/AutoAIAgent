using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoAIAgent.Services
{
    public class CodeImageService
    {
        private static readonly string ImageFolder = "Images";

        public async Task<string> GenerateAsync(string code)
        {
            Directory.CreateDirectory(ImageFolder);
            var path = Path.Combine(ImageFolder, $"code_{DateTime.Now.Ticks}.png");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 50
            });

            var page = await browser.NewPageAsync();

            // Rotate tools: Carbon -> Codeimg.io -> Code Snapshot -> Writecream
            var tools = new[] { "carbon", "codeimg", "codesnapshot", "writecream" };
            foreach (var tool in tools)
            {
                try
                {
                    Console.WriteLine($"[INFO] Using tool: {tool}");

                    switch (tool)
                    {
                        case "carbon":
                            await GenerateWithCarbon(page, code, path);
                            break;
                        case "codeimg":
                            await GenerateWithCodeImg(page, code, path);
                            break;
                        case "codesnapshot":
                            await GenerateWithCodeSnapshot(page, code, path);
                            break;
                        case "writecream":
                            await GenerateWithWritecream(page, code, path);
                            break;
                    }

                    Console.WriteLine($"[SUCCESS] Image generated with {tool}: {path}");
                    return path;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] {tool} failed: {ex.Message}");
                    // Continue to next tool
                }
            }

            throw new Exception("All code image generators failed.");
        }

        // =====================================================
        // 🔥 CARBON (UPDATED FOR CURRENT DOM)
        // =====================================================
        private async Task GenerateWithCarbon(IPage page, string code, string path)
        {
            // ✅ Ensure directory exists
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            await page.GotoAsync("https://carbon.now.sh", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            // Wait until Carbon is fully loaded
            await page.WaitForSelectorAsync("#export-container", new()
            {
                Timeout = 20000
            });

            // ✅ Inject code directly into CodeMirror (NO formatting issues)
            await page.EvaluateAsync(@"
        (code) => {
            const editorEl = document.querySelector('.CodeMirror');
            if (!editorEl || !editorEl.CodeMirror) {
                throw 'CodeMirror not found';
            }

            const cm = editorEl.CodeMirror;
            cm.setValue(code);   // clears + sets clean code
            cm.refresh();
        }
    ", code);

            // Allow Carbon to re-render
            await page.WaitForTimeoutAsync(1500);

            // Screenshot only the export container
            var container = await page.QuerySelectorAsync("#export-container");
            if (container == null)
                throw new Exception("Carbon export container not found");

            await container.ScreenshotAsync(new()
            {
                Path = path
            });

            // ✅ Safety check
            if (!File.Exists(path))
                throw new Exception("Image was not saved to disk");

            Console.WriteLine($"✅ Carbon image saved: {path}");
        }



        // =====================================================
        // 🔥 CODEIMG.IO
        // =====================================================
        private async Task GenerateWithCodeImg(IPage page, string code, string path)
        {
            await page.GotoAsync("https://codeimg.io", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var textarea = await page.WaitForSelectorAsync("textarea", new() { Timeout = 15000 });
            if (textarea == null) throw new Exception("Codeimg.io editor not found");

            await textarea.FillAsync(code);
            await page.WaitForTimeoutAsync(2000);

            var container = await page.QuerySelectorAsync("body");
            if (container == null) throw new Exception("Codeimg.io container not found");

            await container.ScreenshotAsync(new() { Path = path });
        }

        // =====================================================
        // 🔥 CODE SNAPSHOT
        // =====================================================
        private async Task GenerateWithCodeSnapshot(IPage page, string code, string path)
        {
            await page.GotoAsync("https://codesnapshot.dev", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var textarea = await page.WaitForSelectorAsync("textarea", new() { Timeout = 15000 });
            if (textarea == null) throw new Exception("Code Snapshot editor not found");

            await textarea.FillAsync(code);
            await page.WaitForTimeoutAsync(2000);

            var container = await page.QuerySelectorAsync("body");
            if (container == null) throw new Exception("Code Snapshot container not found");

            await container.ScreenshotAsync(new() { Path = path });
        }

        // =====================================================
        // 🔥 WRITECREAM
        // =====================================================
        private async Task GenerateWithWritecream(IPage page, string code, string path)
        {
            await page.GotoAsync("https://writecream.com/code-to-image", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var textarea = await page.WaitForSelectorAsync("textarea", new() { Timeout = 20000 });
            if (textarea == null) throw new Exception("Writecream editor not found");

            await textarea.FillAsync(code);
            await page.WaitForTimeoutAsync(3000);

            var container = await page.QuerySelectorAsync("body");
            if (container == null) throw new Exception("Writecream container not found");

            await container.ScreenshotAsync(new() { Path = path });
        }
    }
}
