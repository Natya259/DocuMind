//create an interface for the document service, this service does all the validations on the files received
using DocuMind.Api.Features.Documents.Upload;
namespace DocuMind.Api.Services.DocumentService;
public interface IDocumentService
{
    Task<UploadDocumentResponseDTO> UploadDocumentsAsync(UploadDocumentRequestDTO request);
}