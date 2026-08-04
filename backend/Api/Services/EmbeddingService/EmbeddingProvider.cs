using System.Linq;
using Microsoft.Extensions.Options;

namespace DocuMind.Api.Services.EmbeddingService;

public class EmbeddingProvider : IEmbeddingProvider
{
    private readonly GoogleAIOptions _options;
    private readonly IGoogleGenAiClient _client;

    public EmbeddingProvider(IOptions<GoogleAIOptions> options, IGoogleGenAiClient client)
    {
        _options = options.Value;
        _client = client;
    }
    
    public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text)
    {
        var values = await _client.EmbedContentAsync(_options.EmbeddingModel, text);
        return values?.Select(value => (float)value).ToList() ?? new List<float>();
    }
}