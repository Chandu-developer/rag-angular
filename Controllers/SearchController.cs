using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rag.Api.Models;
using Rag.Api.Services;

namespace Rag.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly OpenAiEmbeddingService _embeddingService;
        private readonly VectorSearchService _vectorSearchService;
        private readonly OpenAiAnswerService _answerService;

        public SearchController(
            OpenAiEmbeddingService embeddingService,
            VectorSearchService vectorSearchService,
            OpenAiAnswerService answerService)
        {
            _embeddingService = embeddingService;
            _vectorSearchService = vectorSearchService;
            _answerService = answerService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask(
            [FromBody] DocumentSearchRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question is required.");

            // Step 5: Generate the question embedding.
            var embeddings =
                await _embeddingService.GenerateEmbeddingsAsync(
                    new[] { request.Question },
                    cancellationToken);

            var queryEmbedding = embeddings[0];

            // Step 5: Retrieve relevant PDF chunks.
            var chunks = await _vectorSearchService.SearchAsync(
                queryEmbedding,
                request.TopK,
                cancellationToken);

            if (chunks.Count == 0)
            {
                return Ok(new RagAnswerResponse
                {
                    Answer = "No document chunks with embeddings were found.",
                    Sources = new()
                });
            }

            // Prepare the retrieved chunks as AI context.
            var context = string.Join(
                "\n\n",
                chunks.Select((chunk, index) =>
                    $"""
                [Source {index + 1}]
                Document ID: {chunk.DocumentId}
                Page: {chunk.PageNumber}
                Chunk ID: {chunk.Id}
                Text:
                {chunk.ChunkText}
                """));

            // Step 6: Generate an answer grounded in the chunks.
            var answer = await _answerService.GenerateAnswerAsync(
                request.Question,
                context,
                cancellationToken);

            return Ok(new RagAnswerResponse
            {
                Answer = answer,
                Sources = chunks
            });
        }
    }
    }
