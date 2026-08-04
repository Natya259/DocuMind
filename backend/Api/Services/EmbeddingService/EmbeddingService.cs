using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.EmbeddingService;

public class EmbeddingService :IEmbeddingService
{
    private readonly IEmbeddingProvider _embeddingProvider;

    public EmbeddingService(IEmbeddingProvider embeddingProvider)
    {
        _embeddingProvider = embeddingProvider;
    }

    public async Task GenerateEmbeddingsAsync(List<Chunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            chunk.Embedding = (await _embeddingProvider.GenerateEmbeddingAsync(chunk.Text)).ToList();
        }
    }
}