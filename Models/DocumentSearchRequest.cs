namespace Rag.Api.Models
{
    public class DocumentSearchRequest
    {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 5;
    }
}
