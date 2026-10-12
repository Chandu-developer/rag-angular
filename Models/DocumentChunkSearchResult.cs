namespace Rag.Api.Models
{
    public class DocumentChunkSearchResult
    {
        public long Id { get; set; }
        public long DocumentId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string ChunkText { get; set; } = string.Empty;
        public double Distance { get; set; }
    }
}
