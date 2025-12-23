using AutoAIAgent.Services;
using AutoAIAgent.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoAIAgent;

public  class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
            })
            .ConfigureServices((context, services) =>
            {
                var cfg = context.Configuration;

                // 🔐 Config
                var openRouterKey = cfg["OpenRouter:ApiKey"];
                var linkedInEmail = cfg["LinkedIn:Email"];
                var linkedInPassword = cfg["LinkedIn:Password"];

                // 🧠 AI Core
                services.AddSingleton(new OpenRouterService(openRouterKey!));
                services.AddSingleton<CodeGeneratorService>();
                services.AddSingleton<HashtagService>();

                // 🖼 Image
                services.AddSingleton<CodeImageService>();

                // 🌐 LinkedIn
                services.AddSingleton(new LinkedInPoster(
                    linkedInEmail!,
                    linkedInPassword!
                ));

                // 🧾 Job
                services.AddHostedService<OneTimePostService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
}
