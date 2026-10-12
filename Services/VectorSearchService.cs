using Microsoft.Data.SqlClient;
using Rag.Api.Models;
using System.Data;
using System.Text.Json;

namespace Rag.Api.Services;
public class VectorSearchService
{
    private readonly string _connectionString;

    public VectorSearchService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("RagConnectionString")
            ?? throw new InvalidOperationException(
                "DefaultConnection is missing.");
    }

    public async Task<List<DocumentChunkSearchResult>> SearchAsync(
    float[] queryEmbedding,
    int topK = 5,
    CancellationToken cancellationToken = default)
    {
        if (queryEmbedding.Length != 1536)
            throw new ArgumentException(
                "Expected a 1536-dimensional embedding.");

        if (topK < 1 || topK > 20)
            throw new ArgumentOutOfRangeException(nameof(topK));

        var embeddingJson = JsonSerializer.Serialize(queryEmbedding);

        const string sql = """
            DECLARE @QueryEmbedding VECTOR(1536) =
                CAST(@EmbeddingJson AS VECTOR(1536));

            SELECT TOP (@TopK)
                Id,
                DocumentId,
                PageNumber,
                ChunkIndex,
                ChunkText,
                VECTOR_DISTANCE(
                    'cosine', @QueryEmbedding, Embedding
                ) AS Distance
            FROM dbo.DocumentChunks
            WHERE Embedding IS NOT NULL
            ORDER BY Distance ASC;
            """;

        var results = new List<DocumentChunkSearchResult>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@EmbeddingJson", SqlDbType.NVarChar, -1).Value =
            embeddingJson;

        command.Parameters.Add(
            "@TopK", SqlDbType.Int).Value = topK;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentChunkSearchResult
            {
                Id = reader.GetInt64(0),
                DocumentId = reader.GetInt64(1),
                PageNumber = reader.GetInt32(2),
                ChunkIndex = reader.GetInt32(3),
                ChunkText = reader.GetString(4),
                Distance = Convert.ToDouble(reader.GetValue(5))
            });
        }

        return results;
    }
}
