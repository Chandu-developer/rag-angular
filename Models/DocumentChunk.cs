namespace Rag.Api.Models
{
    public class DocumentChunk
    {
        public long Id { get; set; }

        public long DocumentId { get; set; }

        public int PageNumber { get; set; }

        public int ChunkIndex { get; set; }

        public string ChunkText { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public Document Document { get; set; } = null!;
    }
}
