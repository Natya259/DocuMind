using System.Text.Json;
using DocuMind.Api.Common.Models;
using FluentAssertions;

namespace DocuMind.Api.Services.ChunkRepositoryService;

public class ChunkRepositoryTests
{
    [Fact]
    public async Task SaveChunksAsync_CreatesJsonFileForChunks()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;

            var documentId = Guid.NewGuid();
            var chunks = new List<Chunk>
            {
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = 0,
                    Text = "First chunk text",
                    PageNumber = 1
                },
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = 1,
                    Text = "Second chunk text",
                    PageNumber = 2
                }
            };

            var repository = new ChunkRepository();
            var result = await repository.SaveChunksAsync(chunks);

            result.Should().BeTrue();

            var expectedFileName = $"chunks_{documentId}.json";
            var filePath = Path.Combine(tempDirectory, "Storage", "Chunks", expectedFileName);
            File.Exists(filePath).Should().BeTrue();

            var fileContents = await File.ReadAllTextAsync(filePath);
            var loadedChunks = JsonSerializer.Deserialize<List<Chunk>>(fileContents);

            loadedChunks.Should().NotBeNull();
            loadedChunks!.Should().HaveCount(2);
            loadedChunks[0].Text.Should().Be("First chunk text");
            loadedChunks[1].Text.Should().Be("Second chunk text");
            loadedChunks.All(c => c.DocumentId == documentId).Should().BeTrue();
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task LoadChunksAsync_ReturnsChunksFromExistingFile()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var documentId = Guid.NewGuid();
            var expectedFileName = $"chunks_{documentId}.json";
            var storageDirectory = Path.Combine(tempDirectory, "Storage", "Chunks");
            Directory.CreateDirectory(storageDirectory);

            var chunks = new List<Chunk>
            {
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = 0,
                    Text = "Persisted text",
                    PageNumber = 3
                }
            };

            var json = JsonSerializer.Serialize(chunks);
            await File.WriteAllTextAsync(Path.Combine(storageDirectory, expectedFileName), json);

            var repository = new ChunkRepository();
            var loadedChunks = await repository.LoadChunksAsync(expectedFileName);

            loadedChunks.Should().HaveCount(1);
            loadedChunks[0].DocumentId.Should().Be(documentId);
            loadedChunks[0].Text.Should().Be("Persisted text");
            loadedChunks[0].PageNumber.Should().Be(3);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task SaveChunksAsync_ThrowsArgumentException_WhenListIsEmpty()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var repository = new ChunkRepository();

            await Assert.ThrowsAsync<ArgumentException>(() => repository.SaveChunksAsync(new List<Chunk>()));
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task LoadChunksAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var repository = new ChunkRepository();
            var missingFileName = "chunks_missing-document.json";

            await Assert.ThrowsAsync<FileNotFoundException>(() => repository.LoadChunksAsync(missingFileName));
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }
}
