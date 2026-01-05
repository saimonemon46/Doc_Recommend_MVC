using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalDoctorRecommender.Services
{
    public class GroqLLMService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public GroqLLMService(HttpClient http)
        {
            _http = http;

            _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
                ?? throw new Exception("GROQ_API_KEY not found in environment variables");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetNextResponse(
            List<string> conversation,
            int questionCount,
            bool guidanceGiven)
        {
            var prompt = $@"
Conversation so far:
{string.Join("\n", conversation)}

Questions asked so far: {questionCount}
Guidance already given: {guidanceGiven}

Decide the NEXT response.
Rules:
- Ask ONE clarification question if needed
- Ask at most 6 questions total
- Do NOT diagnose
- Do NOT prescribe medication
- Do NOT mention emergencies
- If enough info is available and guidance not given, provide general guidance
- If guidance already given, ask for the user's location
";

            var payload = new
            {
                model = "openai/gpt-oss-120b",
                messages = new[]
                {
                    new { role = "system", content = SYSTEM_PROMPT },
                    new { role = "user", content = prompt }
                }
            };

            var response = await _http.PostAsJsonAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                payload);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Groq API error: {response.StatusCode}");
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            return doc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;
        }

        private const string SYSTEM_PROMPT = @"
You are a medical triage assistant.

Rules:
- Ask only clarification questions
- Ask at most 6 questions total
- Do not diagnose diseases
- Do not prescribe medications
- Do not mention emergency services
- Provide only general, educational health guidance
- After guidance, ask for the user's location
";
    }
}
