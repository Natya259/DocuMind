namespace DocuMind.Api.Services.EmbeddingService;

public interface IEmbeddingProvider
{
    Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string text);
}