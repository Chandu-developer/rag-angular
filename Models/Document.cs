namespace Rag.Api.Models
{
    public class Document
    {
        public long Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedDate { get; set; }

        public string ContentType { get; set; } = string.Empty;
    }
}
