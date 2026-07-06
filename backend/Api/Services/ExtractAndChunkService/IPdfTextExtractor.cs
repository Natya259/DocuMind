//create an instance for PdfTextExtractor class
using DocuMind.Api.Common.Models;
using DocuMind.Api.Features.Documents.Upload;

namespace DocuMind.Api.Services.ExtractAndChunkService;
public interface IPdfTextExtractor
{
    Task<List<Chunk>> ExtractTextFromPdfAsync(UploadedDocuments[] documents);
}