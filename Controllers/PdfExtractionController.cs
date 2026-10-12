using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rag.Api.Data;
using Rag.Api.Services;

namespace Rag.Api.Controllers;

//PdfDocument.Open(filePath) opens the saved PDF.

//document.GetPages() iterates through its pages.

//ContentOrderTextExtractor.GetText(page) extracts text in a more useful reading order.

//Each page's text and page number are added to a list.

//The using statement ensures the PDF document is disposed of correctly

[ApiController]
[Route("api/documents")]
public class PdfExtractionController : ControllerBase
{
    private readonly RagDbContext _db;
    private readonly PdfTextExtractor _extractor;
    private readonly IWebHostEnvironment _environment;

    public PdfExtractionController(
        RagDbContext db,
        PdfTextExtractor extractor,
        IWebHostEnvironment environment)
    {
        _db = db;
        _extractor = extractor;
        _environment = environment;
    }

    [HttpGet("{id:long}/extract-text")]
    public async Task<IActionResult> ExtractText(long id)
    {
        var document = await _db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound("Document not found.");
        }

        var filePath = Path.GetFullPath(
            Path.Combine(
                _environment.ContentRootPath,
                document.FilePath));

        // Ensure the resolved path stays inside the API's storage folder.
        var storageRoot = Path.GetFullPath(
            Path.Combine(
                _environment.ContentRootPath,
                "Documents")) + Path.DirectorySeparatorChar;

        if (!filePath.StartsWith(
                storageRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid document path.");
        }

        try
        {
            var pages = _extractor.ExtractText(filePath);

            return Ok(new
            {
                document.Id,
                document.FileName,
                PageCount = pages.Count,
                Pages = pages
            });
        }
        catch (FileNotFoundException)
        {
            return NotFound("The stored PDF file was not found.");
        }
        catch (Exception ex)
        {
            // Log the exception in production rather than returning
            // internal exception details to the client.
            return BadRequest(
                "Unable to extract text. The PDF may be damaged or encrypted.");
        }
    }
}