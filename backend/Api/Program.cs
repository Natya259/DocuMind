using DocuMind.Api.Features.Documents.Upload;
using DocuMind.Api.Services.ChunkRepositoryService;
using DocuMind.Api.Services.DocumentService;
using DocuMind.Api.Services.EmbeddingService;
using DocuMind.Api.Services.ExtractAndChunkService;
using DocuMind.Api.Services.FileStorageService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();
builder.Services.AddScoped<ITextChunker, TextChunker>();
builder.Services.AddScoped<IChunkRepository, ChunkRepository>();
builder.Services.AddScoped<IEmbeddingProvider, EmbeddingProvider>();
builder.Services.AddScoped<IGoogleGenAiClient, GoogleGenAiClient>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.Configure<GoogleAIOptions>(
builder.Configuration.GetSection(GoogleAIOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{   
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");
app.MapUploadDocumentEndpoint();

app.Run();
