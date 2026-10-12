using Microsoft.EntityFrameworkCore;
using Rag.Api.Data;
using Rag.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddDbContext<RagDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RagConnectionString"));
});
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddScoped<TextChunker>();
builder.Services.AddScoped<VectorSearchService>();
builder.Services.AddHttpClient<OpenAiAnswerService>();
builder.Services.AddHttpClient<OpenAiEmbeddingService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AngularClient");

app.MapControllers();

app.Run();
