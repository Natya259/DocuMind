using DocuMind.Api.Services.DocumentService;

namespace DocuMind.Api.Features.Documents.Upload;
public static class UploadDocumentEndpoint
{
    public static void MapUploadDocumentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/documents", async (HttpRequest request, IDocumentService documentService) =>
        {
            var form = await request.ReadFormAsync();
            Console.WriteLine($"Received {form.Files.Count} files for upload.");

            var uploadRequest = new UploadDocumentRequestDTO
            {
                Files = form.Files.ToList()
            };

            var response = await documentService.UploadDocumentsAsync(uploadRequest);

            if (response.Error != null)
            {
                return Results.BadRequest(response);
            }

            return Results.Created("/api/documents", response);
        }).DisableAntiforgery();
    }
}