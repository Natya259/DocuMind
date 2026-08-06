using DocuMind.Api.Common;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Search;
using DocuMind.Api.Services.ChunkRepositoryService;
using DocuMind.Api.Services.EmbeddingService;

namespace DocuMind.Api.Services.SearchService;

public class SearchResult
{
    public Chunk Chunk { get; set; } = default!;

    public double Score { get; set; }
}

public class SearchService : ISearchService
{
    private readonly IEmbeddingProvider _embeddingProvider;

    private readonly IChunkRepository _chunkRepository;

    public SearchService(
        IEmbeddingProvider embeddingProvider,
        IChunkRepository chunkRepository)
    {
        _embeddingProvider = embeddingProvider;
        _chunkRepository = chunkRepository;
    }

    public async Task<List<SearchResultDTO>> SearchAsync(string question)
    {
        var questionEmbedding =
            await _embeddingProvider.GenerateEmbeddingAsync(question);

        var chunks =
            await _chunkRepository.LoadAllChunksAsync();

        var scoredChunks = new List<SearchResult>();

        foreach (var chunk in chunks)
        {
            if (chunk.Embedding == null)
                continue;

            var similarity = CosineSimilarity.Calculate(
                questionEmbedding,
                chunk.Embedding);

            scoredChunks.Add(new SearchResult
            {
                Chunk = chunk,
                Score = similarity
            });
        }

        var topChunks = scoredChunks
            .OrderByDescending(x => x.Score)
            .Take(5)
            .ToList();

        return topChunks
            .Select(x => new SearchResultDTO
            {
                Score = x.Score,
                Text = x.Chunk.Text,
                PageNumber = x.Chunk.PageNumber ?? 0,
                ChunkIndex = x.Chunk.ChunkIndex,
                DocumentId = x.Chunk.DocumentId.ToString(),
                DocumentName = x.Chunk.DocumentName ?? string.Empty
            }).Where(x => x.Score >= Constants.SimilarityThreshold)
            .ToList();
            }

}