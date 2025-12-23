using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAIAgent.Services
{
    public class CommentReplyService
    {
        private readonly OpenRouterService _ai;

        public CommentReplyService(OpenRouterService ai)
        {
            _ai = ai;
        }

        public async Task<string> ReplyAsync(string comment)
        {
            return await _ai.AskAsync($"""
Reply professionally to this LinkedIn comment:
{comment}
Max 2 lines.
""");
        }
    }

}
