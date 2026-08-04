using DocuMind.Api.Common.Models;

namespace DocuMind.Api.Services.EmbeddingService;
public interface IEmbeddingService
{
    Task GenerateEmbeddingsAsync(List<Chunk> chunks);
}