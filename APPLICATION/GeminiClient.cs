using Google.GenAI;
using Microsoft.Extensions.Options;
using Tweet_Audit.APPLICATION.DTO;

namespace Tweet_Audit.APPLICATION
{
    public class GeminiClient
    {
        private const string ModelName = "gemini-3.1-flash-lite";

        private readonly Client _client;

        public GeminiClient(IOptions<GeminiApiKey> key)
        {
            var apiKey = key.Value.ApiKey;
            _client = new Client(apiKey: apiKey);
        }

        public async Task<string> GeminiAuditAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Models.GenerateContentAsync(
                    model: ModelName,
                    contents: prompt,
                    cancellationToken: cancellationToken
                );

                return response.Text
                    ?? throw new InvalidOperationException("Gemini returned an empty response.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw;
            }
        }
    }
}