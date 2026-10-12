using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rag.Api.Data;
using Rag.Api.Models;
// to upload the pdf documents into local and storing metadata in db
namespace Rag.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly RagDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(
            RagDbContext db,
            IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<IActionResult> Upload(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select a PDF file.");
            }

            if (!file.FileName.EndsWith(
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are allowed.");
            }

            const string folderName = "Documents";

            var folderPath = Path.Combine(
                _environment.ContentRootPath,
                folderName);

            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName);

            var storedFileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                folderPath,
                storedFileName);

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            var document = new Document
            {
                FileName = file.FileName,

                FilePath = Path.Combine(
                    folderName,
                    storedFileName),

                FileSize = file.Length,

                ContentType = file.ContentType,

                UploadedDate = DateTime.UtcNow
            };

            _db.Documents.Add(document);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                document.Id,
                document.FileName,
                Message = "PDF uploaded successfully."
            });
        }
    }
}
