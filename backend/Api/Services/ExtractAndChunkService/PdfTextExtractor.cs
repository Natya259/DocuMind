using System.Text;
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Documents.Upload;
using UglyToad.PdfPig;
namespace DocuMind.Api.Services.ExtractAndChunkService;
// use the PdfPig library to extract text from a PDF file. Implement the ExtractTextFromPdfAsync method of IPdfTextExtractor interface. Return the extracted text as a string.
public class PdfTextExtractor : IPdfTextExtractor
{
    private readonly ITextChunker _textChunker;
    public PdfTextExtractor(ITextChunker textChunker)
    {
        _textChunker = textChunker;
    }   
    public async Task<List<Chunk>> ExtractTextFromPdfAsync(UploadedDocuments[] documents)
    {
        List<Chunk> result = new List<Chunk>();
        foreach (var document in documents)
        {
            using var pdf = PdfDocument.Open(document.FilePath);
            var chunkIndex = 0;

            foreach (var page in pdf.GetPages())
            {
                var pageText = page.Text?.Trim();
                if (string.IsNullOrWhiteSpace(pageText))
                {
                    continue;
                }

                var pageChunks = _textChunker.ChunkText(document.DocumentId, pageText, page.Number);
                foreach (var chunk in pageChunks)
                {
                    chunk.ChunkIndex = chunkIndex++;
                    result.Add(chunk);
                }
            }
        }

        return result;

       

    }
}