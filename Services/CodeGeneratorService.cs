using System;
using System.Threading.Tasks;

namespace AutoAIAgent.Services
{
    public class CodeGeneratorService
    {
        private readonly OpenRouterService _ai;

        public CodeGeneratorService(OpenRouterService ai)
        {
            _ai = ai;
        }

        /// <summary>
        /// Generates a concise, complete C# snippet with topic and LinkedIn post text.
        /// </summary>
        public async Task<(string topic, string code, string postText)> GenerateAsync()
        {
            // 1️⃣ Generate a short viral topic
            var topicPrompt = "Give ONE short, viral, and highly useful FullStack .NET tip topic (max 6 words)";
            var topic = (await _ai.AskAsync(topicPrompt))?.Trim() ?? "FullStack .NET Tip";

            // 2️⃣ Generate a short but complete code snippet (approx 20-30 lines)
            string code = string.Empty;
            int attempts = 0;

            while (attempts < 3)
            {
                var codePrompt = $"""
Generate a COMPLETE, SHORT, COMPILABLE C# CODE snippet (~30-50 lines).

Rules:
- Include `using` statements and a namespace.
- Include ONE public class.
- Must be fully compilable.
- Keep it concise and clear.
- No explanations or markdown.

Topic:
{topic}
""";

                code = (await _ai.AskAsync(codePrompt))?.Trim() ?? "";

                if (IsCodeComplete(code))
                    break;

                attempts++;
            }

            if (!IsCodeComplete(code))
                throw new Exception("AI failed to generate a short, complete C# code snippet.");

            // 3️⃣ Generate short LinkedIn post text
            var postTextPrompt = $"""
Write a concise LinkedIn post (5-10 lines) about:
Topic: {topic}
Explain the usefulness of the code snippet.
Optional emoji 🚀 or 📌
""";

            var postText = (await _ai.AskAsync(postTextPrompt))?.Trim() ?? "";

            return (topic, code, postText);
        }

        /// <summary>
        /// Quick check if the generated code is complete
        /// </summary>
        private bool IsCodeComplete(string code)
        {
            return
                !string.IsNullOrWhiteSpace(code) &&
                code.Contains("namespace ") &&
                code.Contains("class ") &&
                code.TrimEnd().EndsWith("}");
        }
    }
}
