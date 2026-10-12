namespace Rag.Api.Models
{
    public class RagAnswerResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<DocumentChunkSearchResult> Sources { get; set; } = new();
    }
}
