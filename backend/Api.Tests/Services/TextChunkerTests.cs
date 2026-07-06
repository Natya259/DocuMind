using System.Linq;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Services.ExtractAndChunkService;
using FluentAssertions;

namespace Api.Tests.Services;

public class TextChunkerTests
{
    [Fact]
    public void ChunkText_ReturnsSingleChunk_WhenTextShorterThanMaxSize()
    {
        var chunker = new TextChunker();
        var documentId = Guid.NewGuid();
        var text = "This is a short text sample.";

        var chunks = chunker.ChunkText(documentId, text, maxChunkSize: 100, overlap: 10);

        chunks.Should().ContainSingle();
        chunks[0].ChunkIndex.Should().Be(0);
        chunks[0].DocumentId.Should().Be(documentId);
        chunks[0].Text.Should().Be(text);
    }

    [Fact]
    public void ChunkText_SplitsLongText_AtWordBoundary()
    {
        var chunker = new TextChunker();
        var documentId = Guid.NewGuid();
        var text = string.Join(' ', Enumerable.Range(0, 20).Select(i => $"word{i}"));
        var maxChunkSize = 20;
        var overlap = 5;

        var chunks = chunker.ChunkText(documentId, text, maxChunkSize, overlap);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Select(c => c.ChunkIndex).Should().BeInAscendingOrder();
        chunks.Should().OnlyContain(c => c.DocumentId == documentId);
        chunks.Should().OnlyContain(c => c.Text.Length <= maxChunkSize);
        chunks[0].Text.Should().Be(text.Substring(0, chunks[0].Text.Length));
        chunks[1].Text.Should().Contain(text.Substring(chunks[0].Text.Length - overlap, overlap));
    }

    [Fact]
    public void ChunkText_UsesMaxSize_WhenNoSpaceBeforeBoundary()
    {
        var chunker = new TextChunker();
        var documentId = Guid.NewGuid();
        var text = new string('a', 70);

        var chunks = chunker.ChunkText(documentId, text, maxChunkSize: 20, overlap: 5);

        chunks.Should().HaveCountGreaterThan(1);
        chunks.Should().OnlyContain(c => c.Text.All(ch => ch == 'a'));
        chunks.Select(c => c.ChunkIndex).Should().BeInAscendingOrder();
        chunks.Last().Text.Length.Should().BeLessThan(21);
    }
}
