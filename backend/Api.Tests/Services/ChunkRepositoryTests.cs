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
    public async Task LoadAllChunksAsync_ReturnsChunksFromExistingFiles()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var storageDirectory = Path.Combine(tempDirectory, "Storage", "Chunks");
            Directory.CreateDirectory(storageDirectory);

            var firstDocumentId = Guid.NewGuid();
            var firstChunks = new List<Chunk>
            {
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = firstDocumentId,
                    ChunkIndex = 0,
                    Text = "First document chunk",
                    PageNumber = 1
                }
            };

            var secondDocumentId = Guid.NewGuid();
            var secondChunks = new List<Chunk>
            {
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = secondDocumentId,
                    ChunkIndex = 0,
                    Text = "Second document chunk",
                    PageNumber = 2
                }
            };

            await File.WriteAllTextAsync(Path.Combine(storageDirectory, $"chunks_{firstDocumentId}.json"), JsonSerializer.Serialize(firstChunks));
            await File.WriteAllTextAsync(Path.Combine(storageDirectory, $"chunks_{secondDocumentId}.json"), JsonSerializer.Serialize(secondChunks));

            var repository = new ChunkRepository();
            var loadedChunks = await repository.LoadAllChunksAsync();

            loadedChunks.Should().HaveCount(2);
            loadedChunks.Select(c => c.DocumentId).Should().BeEquivalentTo(new[] { firstDocumentId, secondDocumentId });
            loadedChunks.Select(c => c.Text).Should().Contain(new[] { "First document chunk", "Second document chunk" });
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
    public async Task LoadAllChunksAsync_ReturnsEmptyList_WhenNoChunkFilesExist()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var repository = new ChunkRepository();

            var loadedChunks = await repository.LoadAllChunksAsync();

            loadedChunks.Should().BeEmpty();
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }
}
