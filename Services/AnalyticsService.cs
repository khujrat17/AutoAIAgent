using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAIAgent.Services
{
    public class AnalyticsService
    {
        public async Task SaveAsync(int likes, int comments)
        {
            var data = $"{DateTime.Now}: Likes={likes}, Comments={comments}";
            await File.AppendAllTextAsync("Analytics/stats.txt", data + "\n");
        }
    }

}
