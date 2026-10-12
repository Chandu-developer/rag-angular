using Rag.Api.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Rag.Api.Services;

public class PdfTextExtractor
{
    public List<ExtractedPage> ExtractText(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The PDF file was not found.", filePath);
        }

        var extractedPages = new List<ExtractedPage>();

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);

            extractedPages.Add(new ExtractedPage
            {
                PageNumber = page.Number,
                Text = text.Trim()
            });
        }

        return extractedPages;
    }
}