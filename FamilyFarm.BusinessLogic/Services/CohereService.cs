using FamilyFarm.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class CohereService :ICohereService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string CohereApiKey = "oJW30Myk7DDQEglvMxyEZyI5gSTCyb7DcNTRVDLt";
        private const string CohereEndpoint = "https://api.cohere.ai/v1/generate";

        public CohereService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsAgricultureRelatedAsync(string contentToCheck)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CohereApiKey);

            var payload = new
            {
                model = "command-r-plus",
                prompt = $"Hãy trả lời 'Yes' nếu nội dung sau liên quan đến lĩnh vực nông nghiệp, nếu không thì trả lời 'No'. Nội dung: \"{contentToCheck}\"",
                max_tokens = 5,
                temperature = 0.2,
                stop_sequences = new[] { "\n" }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(CohereEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Lỗi khi gọi Cohere API");

            var jsonDoc = JsonDocument.Parse(responseContent);
            var result = jsonDoc.RootElement.GetProperty("generations")[0].GetProperty("text").GetString();

            return result.Trim().ToLower().Contains("yes");
        }
    }
}
