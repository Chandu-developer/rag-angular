using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Rag.Api.Services;

public class OpenAiEmbeddingService
{
    private readonly HttpClient _httpClient;

    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        var apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");
        }

        _httpClient.BaseAddress =
            new Uri("https://api.openai.com/v1/");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
            return new List<float[]>();

        var request = new EmbeddingRequest
        {
            Model = "text-embedding-3-small",
            Input = texts
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "embeddings",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine($"OpenAI error: {errorBody}");

            Console.WriteLine(
                $"Retry-After: {response.Headers.RetryAfter}");

            throw new HttpRequestException(
                $"Embedding request failed: {(int)response.StatusCode}. " +
                errorBody);
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<EmbeddingResponse>(
                cancellationToken: cancellationToken);

        if (result?.Data == null ||
            result.Data.Count != texts.Count)
        {
            throw new InvalidOperationException(
                "The embedding API returned an unexpected result.");
        }

        var embeddings = new float[texts.Count][];

        foreach (var item in result.Data)
        {
            if (item.Index < 0 || item.Index >= texts.Count)
            {
                throw new InvalidOperationException(
                    "Invalid embedding index returned by the API.");
            }

            embeddings[item.Index] = item.Embedding;
        }

        if (embeddings.Any(e => e == null || e.Length != 1536))
        {
            throw new InvalidOperationException(
                "Expected a 1536-dimensional embedding.");
        }

        return embeddings.ToList();
    }

    private sealed class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public IReadOnlyList<string> Input { get; set; } =
            Array.Empty<string>();
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingItem> Data { get; set; } = new();
    }

    private sealed class EmbeddingItem
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}