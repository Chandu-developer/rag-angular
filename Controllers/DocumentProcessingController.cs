using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rag.Api.Data;
using Rag.Api.Models;
using Rag.Api.Services;

namespace Rag.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentProcessingController : ControllerBase
{
    private readonly RagDbContext _db;
    private readonly PdfTextExtractor _extractor;
    private readonly TextChunker _chunker;
    private readonly IWebHostEnvironment _environment;

    public DocumentProcessingController(
        RagDbContext db,
        PdfTextExtractor extractor,
        TextChunker chunker,
        IWebHostEnvironment environment)
    {
        _db = db;
        _extractor = extractor;
        _chunker = chunker;
        _environment = environment;
    }

    [HttpPost("{id:long}/process")]
    public async Task<IActionResult> Process(long id)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
            return NotFound("Document not found.");

        var filePath = Path.GetFullPath(
            Path.Combine(
                _environment.ContentRootPath,
                document.FilePath));

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

        if (!System.IO.File.Exists(filePath))
            return NotFound("Stored PDF file not found.");

        List<ExtractedPage> pages;

        try
        {
            pages = _extractor.ExtractText(filePath);
        }
        catch (Exception)
        {
            return BadRequest(
                "Unable to read this PDF. It may be damaged or encrypted.");
        }

        var newChunks = new List<DocumentChunk>();

        foreach (var page in pages)
        {
            var chunks = _chunker.Split(page.Text);

            for (var i = 0; i < chunks.Count; i++)
            {
                newChunks.Add(new DocumentChunk
                {
                    DocumentId = document.Id,
                    PageNumber = page.PageNumber,
                    ChunkIndex = i,
                    ChunkText = chunks[i],
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        if (newChunks.Count == 0)
        {
            return BadRequest(
                "No text was extracted. This may be a scanned PDF requiring OCR.");
        }

        var oldChunks = await _db.DocumentChunks
            .Where(c => c.DocumentId == document.Id)
            .ToListAsync();

        _db.DocumentChunks.RemoveRange(oldChunks);
        _db.DocumentChunks.AddRange(newChunks);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            DocumentId = document.Id,
            document.FileName,
            PageCount = pages.Count,
            ChunkCount = newChunks.Count,
            Message = "PDF text extracted and chunks stored successfully."
        });
    }
}