using System.Text;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.ExtractAndChunkService;
using FluentAssertions;
using Moq;

namespace Api.Tests.Services;

public class PdfTextExtractorTests
{
    [Fact]
    public async Task ExtractTextFromPdfAsync_ReturnsChunksFromPdfContent()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Environment.CurrentDirectory = tempDirectory;
            var pdfFilePath = Path.Combine(tempDirectory, "sample.pdf");
            await File.WriteAllBytesAsync(pdfFilePath, CreateMinimalPdfBytes("Hello world"));

            var documentId = Guid.NewGuid();
            var documents = new[]
            {
                new UploadedDocuments
                {
                    DocumentId = documentId,
                    FileName = "sample.pdf",
                    FilePath = pdfFilePath,
                    Status = "Uploaded"
                }
            };

            var extractedChunks = new List<Chunk>
            {
                new Chunk
                {
                    ChunkId = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = 0,
                    Text = "Hello world",
                    PageNumber = 1
                }
            };

            var textChunkerMock = new Mock<ITextChunker>();
            textChunkerMock
                .Setup(x => x.ChunkText(documentId, It.Is<string>(text => text.Contains("Hello world")), It.IsAny<int>()))
                .Returns(extractedChunks);

            var extractor = new PdfTextExtractor(textChunkerMock.Object);
            var result = await extractor.ExtractTextFromPdfAsync(documents);

            result.Should().BeSameAs(extractedChunks);
            textChunkerMock.Verify(x => x.ChunkText(documentId, It.Is<string>(text => text.Contains("Hello world")), 1), Times.Once);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task ExtractTextFromPdfAsync_ReturnsEmptyList_WhenNoDocumentsAreProvided()
    {
        var textChunkerMock = new Mock<ITextChunker>();
        var extractor = new PdfTextExtractor(textChunkerMock.Object);

        var result = await extractor.ExtractTextFromPdfAsync(Array.Empty<UploadedDocuments>());

        result.Should().BeEmpty();
        textChunkerMock.Verify(x => x.ChunkText(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    private static byte[] CreateMinimalPdfBytes(string text)
    {
        static string EscapePdfString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        var escapedText = EscapePdfString(text);
        var content = $"BT /F1 24 Tf 100 700 Td ({escapedText}) Tj ET";

        var lines = new List<string>
        {
            "%PDF-1.4",
            "1 0 obj",
            "<< /Type /Catalog /Pages 2 0 R >>",
            "endobj",
            "2 0 obj",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "endobj",
            "3 0 obj",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            "endobj",
            "4 0 obj",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>",
            "stream",
            content,
            "endstream",
            "endobj",
            "5 0 obj",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            "endobj"
        };

        var bytes = new List<byte>();
        var offsets = new List<long>();

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].EndsWith("obj") && (lines[i].StartsWith("1 ") || lines[i].StartsWith("2 ") || lines[i].StartsWith("3 ") || lines[i].StartsWith("4 ") || lines[i].StartsWith("5 ")))
            {
                offsets.Add(bytes.Count);
            }

            var lineBytes = Encoding.ASCII.GetBytes(lines[i] + "\r\n");
            bytes.AddRange(lineBytes);
        }

        var xrefOffset = bytes.Count;
        var xrefLines = new List<string>
        {
            "xref",
            "0 6",
            "0000000000 65535 f ",
            string.Format("{0:0000000000} 00000 n ", offsets[0]),
            string.Format("{0:0000000000} 00000 n ", offsets[1]),
            string.Format("{0:0000000000} 00000 n ", offsets[2]),
            string.Format("{0:0000000000} 00000 n ", offsets[3]),
            string.Format("{0:0000000000} 00000 n ", offsets[4]),
            "trailer << /Root 1 0 R /Size 6 >>",
            "startxref",
            xrefOffset.ToString(),
            "%%EOF"
        };

        foreach (var line in xrefLines)
        {
            bytes.AddRange(Encoding.ASCII.GetBytes(line + "\r\n"));
        }

        return bytes.ToArray();
    }
}
