using Polly;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AutoAIAgent.Services
{
    public class OpenRouterService : IDisposable
    {
        private readonly HttpClient _http;
        private readonly AsyncRetryPolicy _retryPolicy;

        public OpenRouterService(string apiKey)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://openrouter.ai/api/v1/"),
                Timeout = TimeSpan.FromSeconds(60)
            };

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            _http.DefaultRequestHeaders.Add("Referer", "https://localhost");
            _http.DefaultRequestHeaders.Add("X-Title", "Auto LinkedIn AI Agent");

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<JsonException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(2 * retry)
                );
        }

        // ============================================================
        // SAFE GENERIC ASK (FIXES YOUR ERROR)
        // ============================================================
        public async Task<string> AskAsync(string prompt)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var body = new
                {
                    model = "openai/gpt-4.1-mini",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 300
                };

                var response = await _http.PostAsJsonAsync("chat/completions", body);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"OpenRouter HTTP {(int)response.StatusCode}: {raw}");
                }

                using var json = JsonDocument.Parse(raw);
                var root = json.RootElement;

                // 🛑 SAFETY CHECKS (THIS FIXES YOUR CRASH)
                if (!root.TryGetProperty("choices", out var choices) ||
                    choices.ValueKind != JsonValueKind.Array ||
                    choices.GetArrayLength() == 0)
                {
                    throw new Exception($"OpenRouter returned no choices: {raw}");
                }

                var choice = choices[0];

                if (!choice.TryGetProperty("message", out var message) ||
                    !message.TryGetProperty("content", out var content))
                {
                    throw new Exception($"OpenRouter response missing content: {raw}");
                }

                return content.GetString()?.Trim() ?? string.Empty;
            });
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
