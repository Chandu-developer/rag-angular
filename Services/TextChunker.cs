namespace Rag.Api.Services;

public class TextChunker
{
    // chunksize: keeps 1000 chars for a chunk
    //overlap: starts with prev 150 chars, so that will become easy while 
    public List<string> Split(string text, int chunkSize = 1000, int overlap = 150)
    {
        if (chunkSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize));

        if (overlap < 0 || overlap >= chunkSize)
            throw new ArgumentOutOfRangeException(nameof(overlap));

        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        text = text.Trim();

        var start = 0;

        while (start < text.Length)
        {
            var length = Math.Min(
                chunkSize,
                text.Length - start);

            var chunk = text.Substring(start, length).Trim();

            if (!string.IsNullOrWhiteSpace(chunk))
                chunks.Add(chunk);

            if (start + length >= text.Length)
                break;

            start += chunkSize - overlap;
        }

        return chunks;
    }
}