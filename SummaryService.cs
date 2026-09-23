using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeetingAssistant
{
    public class SummaryService
    {
        private readonly HttpClient client = new HttpClient();
        private static readonly string ApiKey =
          Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? "";
        private const string ApiUrl = "https://api.deepseek.com/chat/completions";

        public async Task<string> SummarizeAsync(string meetingText)
        {
            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "你是一个会议纪要助手，请用简洁的要点总结下面的会议内容。" },
                    new { role = "user", content = meetingText }
                }
            };

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ApiKey);

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PostAsync(ApiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseText);
                string summary = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return summary ?? "[总结为空]";
            }
            catch (Exception ex)
            {
                return "[总结失败] " + ex.Message;
            }
        }
    }
}