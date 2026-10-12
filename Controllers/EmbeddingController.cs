using Microsoft.AspNetCore.Mvc;
using Microsoft.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Rag.Api.Data;
using Rag.Api.Services;

namespace Rag.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class EmbeddingController : ControllerBase
{
    private const int BatchSize = 50;

    private readonly RagDbContext _db;
    private readonly OpenAiEmbeddingService _embeddingService;
    private readonly IConfiguration _configuration;

    public EmbeddingController(
        RagDbContext db,
        OpenAiEmbeddingService embeddingService,
        IConfiguration configuration)
    {
        _db = db;
        _embeddingService = embeddingService;
        _configuration = configuration;
    }

    [HttpPost("{id:long}/generate-embeddings")]
    public async Task<IActionResult> GenerateEmbeddings(
        long id,
        CancellationToken cancellationToken)
    {
        var documentExists = await _db.Documents
            .AnyAsync(d => d.Id == id, cancellationToken);

        if (!documentExists)
            return NotFound("Document not found.");

        var chunks = await _db.DocumentChunks
            .AsNoTracking()
            .Where(c => c.DocumentId == id)
            .OrderBy(c => c.PageNumber)
            .ThenBy(c => c.ChunkIndex)
            .Select(c => new
            {
                c.Id,
                c.ChunkText
            })
            .ToListAsync(cancellationToken);

        if (chunks.Count == 0)
        {
            return BadRequest(
                "No chunks found. Process the PDF first.");
        }

        var connectionString =
            _configuration.GetConnectionString("RagConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "RagDatabase connection string is missing.");
        }

        var processedCount = 0;

        await using var connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync(cancellationToken);

        for (var offset = 0; offset < chunks.Count; offset += BatchSize)
        {
            var batch = chunks
                .Skip(offset)
                .Take(BatchSize)
                .ToList();

            var texts = batch
                .Select(c => c.ChunkText)
                .ToList();

            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(
                    texts,
                    cancellationToken);

            for (var i = 0; i < batch.Count; i++)
            {
                await using var command = connection.CreateCommand();

                command.CommandText = """
                    UPDATE dbo.DocumentChunks
                    SET Embedding = @Embedding
                    WHERE Id = @Id AND DocumentId = @DocumentId;
                    """;

                command.Parameters.AddWithValue(
                    "@Id", batch[i].Id);

                command.Parameters.AddWithValue(
                    "@DocumentId", id);

                command.Parameters.Add(new SqlParameter(
                    "@Embedding",
                    SqlDbTypeExtensions.Vector)
                {
                    Value = new SqlVector<float>(
                        embeddings[i].AsMemory())
                });

                await command.ExecuteNonQueryAsync(cancellationToken);

                processedCount++;
            }
        }

        return Ok(new
        {
            DocumentId = id,
            TotalChunks = chunks.Count,
            EmbeddingsGenerated = processedCount,
            Dimensions = 1536,
            Message = "Embeddings generated and stored successfully."
        });
    }
}