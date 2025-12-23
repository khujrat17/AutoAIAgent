using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAIAgent.Services
{
    public class HashtagService
    {
        private readonly OpenRouterService _ai;

        public HashtagService(OpenRouterService ai)
        {
            _ai = ai;
        }

        public async Task<string> GenerateAsync(string topic)
        {
            return await _ai.AskAsync($"""
Generate 10 LinkedIn hashtags for:
{topic}
Comma separated.
""");
        }
    }

}
