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
        string extractedTexts;
        foreach(var document in documents)
        {
            
            var text = new StringBuilder();

            using (var pdf = PdfDocument.Open(document.FilePath))
            {
                //get size of pdf file in bytes
                var fileInfo = new FileInfo(document.FilePath);
                foreach (var page in pdf.GetPages())
                {
                    text.AppendLine(page.Text);
                }
            }
            extractedTexts = text.ToString();
             // send extractedTexts to TextChunker class
            result = _textChunker.ChunkText(document.DocumentId, extractedTexts);

        }
        return result;

       

    }
}