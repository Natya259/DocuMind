using DocuMind.Api.Common.Models;
using DocuMind.Api.Services.EmbeddingService;
using FluentAssertions;
using Moq;

namespace Api.Tests.Services;

public class EmbeddingServiceTests
{
    [Fact]
    public async Task GenerateEmbeddingsAsync_SetsEmbedding_ForEachChunk()
    {
        var provider = new Mock<IEmbeddingProvider>();
        var chunk1 = new Chunk { ChunkId = Guid.NewGuid(), DocumentId = Guid.NewGuid(), ChunkIndex = 0, Text = "first chunk" };
        var chunk2 = new Chunk { ChunkId = Guid.NewGuid(), DocumentId = Guid.NewGuid(), ChunkIndex = 1, Text = "second chunk" };

        provider
            .Setup(p => p.GenerateEmbeddingAsync("first chunk"))
            .ReturnsAsync(new List<float> { 0.1f, 0.2f, 0.3f });
        provider
            .Setup(p => p.GenerateEmbeddingAsync("second chunk"))
            .ReturnsAsync(new List<float> { 1.0f, 2.0f });

        var service = new EmbeddingService(provider.Object);
        var chunks = new List<Chunk> { chunk1, chunk2 };

        await service.GenerateEmbeddingsAsync(chunks);

        chunk1.Embedding.Should().Equal(new[] { 0.1f, 0.2f, 0.3f });
        chunk2.Embedding.Should().Equal(new[] { 1.0f, 2.0f });

        provider.Verify(p => p.GenerateEmbeddingAsync("first chunk"), Times.Once);
        provider.Verify(p => p.GenerateEmbeddingAsync("second chunk"), Times.Once);
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_SetsEmptyEmbedding_WhenProviderReturnsNoValues()
    {
        var provider = new Mock<IEmbeddingProvider>();
        var chunk = new Chunk { ChunkId = Guid.NewGuid(), DocumentId = Guid.NewGuid(), ChunkIndex = 0, Text = "empty vector" };

        provider
            .Setup(p => p.GenerateEmbeddingAsync(chunk.Text))
            .ReturnsAsync(Array.Empty<float>());

        var service = new EmbeddingService(provider.Object);
        var chunks = new List<Chunk> { chunk };

        await service.GenerateEmbeddingsAsync(chunks);

        chunk.Embedding.Should().NotBeNull();
        chunk.Embedding.Should().BeEmpty();
        provider.Verify(p => p.GenerateEmbeddingAsync(chunk.Text), Times.Once);
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_DoesNothing_WhenNoChunksAreProvided()
    {
        var provider = new Mock<IEmbeddingProvider>();
        var service = new EmbeddingService(provider.Object);

        var chunks = new List<Chunk>();
        await service.GenerateEmbeddingsAsync(chunks);

        provider.Verify(p => p.GenerateEmbeddingAsync(It.IsAny<string>()), Times.Never);
        chunks.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateEmbeddingsAsync_ReplacesExistingEmbedding_WhenEmbeddingAlreadyPresent()
    {
        var provider = new Mock<IEmbeddingProvider>();
        var chunk = new Chunk
        {
            ChunkId = Guid.NewGuid(),
            DocumentId = Guid.NewGuid(),
            ChunkIndex = 0,
            Text = "updated chunk",
            Embedding = new List<float> { 9.9f }
        };

        provider
            .Setup(p => p.GenerateEmbeddingAsync(chunk.Text))
            .ReturnsAsync(new List<float> { 0.5f, 0.6f });

        var service = new EmbeddingService(provider.Object);
        var chunks = new List<Chunk> { chunk };

        await service.GenerateEmbeddingsAsync(chunks);

        chunk.Embedding.Should().Equal(new[] { 0.5f, 0.6f });
        provider.Verify(p => p.GenerateEmbeddingAsync(chunk.Text), Times.Once);
    }
}
