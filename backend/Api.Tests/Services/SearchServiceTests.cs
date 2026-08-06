using DocuMind.Api.Common;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Search;
using DocuMind.Api.Services.ChunkRepositoryService;
using DocuMind.Api.Services.EmbeddingService;
using DocuMind.Api.Services.SearchService;
using FluentAssertions;
using Moq;

namespace DocuMind.Api.Services.SearchService;

public class SearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsTopChunks_OrderedByScore()
    {
        // Arrange
        var embeddingProvider = new Mock<IEmbeddingProvider>();
        embeddingProvider
            .Setup(x => x.GenerateEmbeddingAsync("search text"))
            .ReturnsAsync(new float[] { 1f, 0f });

        var chunks = new List<Chunk>
        {
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                ChunkIndex = 0,
                Text = "Low score chunk",
                PageNumber = 1,
                Embedding = new float[] { 0f, 1f },
                DocumentName = "Doc1"
            },
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                ChunkIndex = 1,
                Text = "High score chunk",
                PageNumber = 2,
                Embedding = new float[] { 1f, 0f },
                DocumentName = "Doc2"
            }
        };

        var chunkRepository = new Mock<IChunkRepository>();
        chunkRepository
            .Setup(x => x.LoadAllChunksAsync())
            .ReturnsAsync(chunks);

        var service = new SearchService(embeddingProvider.Object, chunkRepository.Object);

        // Act
        var results = await service.SearchAsync("search text");

        // Assert
        results.Should().HaveCount(1);
        results[0].Text.Should().Be("High score chunk");
        results[0].Score.Should().BeApproximately(1.0, 1e-9);
        results[0].DocumentId.Should().Be(chunks[1].DocumentId.ToString());
        results[0].DocumentName.Should().Be("Doc2");
    }

    [Fact]
    public async Task SearchAsync_ExcludesChunks_BelowSimilarityThreshold()
    {
        // Arrange
        var embeddingProvider = new Mock<IEmbeddingProvider>();
        embeddingProvider
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 1f, 0f });

        var chunks = new List<Chunk>
        {
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                ChunkIndex = 0,
                Text = "Below threshold",
                PageNumber = 1,
                Embedding = new float[] { 0.1f, 0.9f },
                DocumentName = "Doc1"
            },
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                ChunkIndex = 1,
                Text = "Above threshold",
                PageNumber = 2,
                Embedding = new float[] { 1f, 0f },
                DocumentName = "Doc2"
            }
        };

        var chunkRepository = new Mock<IChunkRepository>();
        chunkRepository
            .Setup(x => x.LoadAllChunksAsync())
            .ReturnsAsync(chunks);

        var service = new SearchService(embeddingProvider.Object, chunkRepository.Object);

        // Act
        var results = await service.SearchAsync("search text");

        // Assert
        results.Should().ContainSingle();
        results[0].Text.Should().Be("Above threshold");
        results[0].Score.Should().BeApproximately(1.0, 1e-9);
    }

    [Fact]
    public async Task SearchAsync_IgnoresChunks_WithNullEmbeddings()
    {
        // Arrange
        var embeddingProvider = new Mock<IEmbeddingProvider>();
        embeddingProvider
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(new float[] { 1f, 0f });

        var chunks = new List<Chunk>
        {
            new Chunk
            {
                ChunkId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                ChunkIndex = 0,
                Text = "Null embedding chunk",
                PageNumber = 1,
                Embedding = null,
                DocumentName = "Doc1"
            }
        };

        var chunkRepository = new Mock<IChunkRepository>();
        chunkRepository
            .Setup(x => x.LoadAllChunksAsync())
            .ReturnsAsync(chunks);

        var service = new SearchService(embeddingProvider.Object, chunkRepository.Object);

        // Act
        var results = await service.SearchAsync("search text");

        // Assert
        results.Should().BeEmpty();
    }
}
