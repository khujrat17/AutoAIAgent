using AutoAIAgent.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AutoAIAgent.Jobs
{
    public class OneTimePostService : BackgroundService
    {
        private readonly CodeGeneratorService _codeGenerator;
        private readonly CodeImageService _imageService;
        private readonly HashtagService _hashtagService;
        private readonly LinkedInPoster _poster;
        private readonly ILogger<OneTimePostService> _logger;

        public OneTimePostService(
            CodeGeneratorService codeGenerator,
            CodeImageService imageService,
            HashtagService hashtagService,
            LinkedInPoster poster,
            ILogger<OneTimePostService> logger)
        {
            _codeGenerator = codeGenerator;
            _imageService = imageService;
            _hashtagService = hashtagService;
            _poster = poster;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 LinkedIn AI Agent started");

            try
            {
                // 1️⃣ Generate topic + code + post text
                var (topic, code, postText) = await _codeGenerator.GenerateAsync();
                _logger.LogInformation($"Topic generated: {topic}");

                // 2️⃣ Generate code image (auto rotate tools)
                var imagePath = await _imageService.GenerateAsync(code);
                _logger.LogInformation($"Image generated: {imagePath}");

                // 3️⃣ Generate hashtags
                var hashtags = await _hashtagService.GenerateAsync(topic);

                // 4️⃣ Compose full caption
                var caption = $"{postText}\n\n{hashtags} #dotnet #backend #softwareengineering";

                // 5️⃣ Post to LinkedIn
                await _poster.PostAsync(caption, imagePath);

                _logger.LogInformation("✅ Post published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to post");
            }

            _logger.LogInformation("🛑 OneTimePostService completed");
        }
    }
}
