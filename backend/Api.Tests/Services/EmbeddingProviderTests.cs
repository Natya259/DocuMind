using DocuMind.Api.Services.EmbeddingService;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Api.Tests.Services;

public class EmbeddingProviderTests
{
    [Fact]
    public async Task GenerateEmbeddingAsync_ReturnsFloatList_WhenClientReturnsDoubles()
    {
        var options = Options.Create(new GoogleAIOptions
        {
            ApiKey = "test-key",
            EmbeddingModel = "test-model"
        });

        var client = new Mock<IGoogleGenAiClient>();
        client
            .Setup(x => x.EmbedContentAsync("test-model", "hello world"))
            .ReturnsAsync(new List<double> { 0.1, 0.2, 0.3 });

        var provider = new EmbeddingProvider(options, client.Object);

        var embeddings = await provider.GenerateEmbeddingAsync("hello world");

        embeddings.Should().BeEquivalentTo(new[] { 0.1f, 0.2f, 0.3f });
        client.Verify(x => x.EmbedContentAsync("test-model", "hello world"), Times.Once);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ReturnsEmptyList_WhenClientReturnsNull()
    {
        var options = Options.Create(new GoogleAIOptions
        {
            ApiKey = "test-key",
            EmbeddingModel = "test-model"
        });

        var client = new Mock<IGoogleGenAiClient>();
        client
            .Setup(x => x.EmbedContentAsync("test-model", "empty"))
            .ReturnsAsync((IReadOnlyList<double>?)null);

        var provider = new EmbeddingProvider(options, client.Object);

        var embeddings = await provider.GenerateEmbeddingAsync("empty");

        embeddings.Should().BeEmpty();
        client.Verify(x => x.EmbedContentAsync("test-model", "empty"), Times.Once);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ReturnsEmptyList_WhenClientReturnsEmptyCollection()
    {
        var options = Options.Create(new GoogleAIOptions
        {
            ApiKey = "test-key",
            EmbeddingModel = "test-model"
        });

        var client = new Mock<IGoogleGenAiClient>();
        client
            .Setup(x => x.EmbedContentAsync("test-model", "nothing"))
            .ReturnsAsync(Array.Empty<double>());

        var provider = new EmbeddingProvider(options, client.Object);

        var embeddings = await provider.GenerateEmbeddingAsync("nothing");

        embeddings.Should().BeEmpty();
        client.Verify(x => x.EmbedContentAsync("test-model", "nothing"), Times.Once);
    }
}
