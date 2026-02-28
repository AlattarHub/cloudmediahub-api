using CloudMediaHub.Api.Data;
using CloudMediaHub.Api.Data.Entities;
using CloudMediaHub.Api.Models;
using CloudMediaHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CloudMediaHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly AppDbContext _db;
        private readonly IFileValidator _fileValidator;
        private readonly ILogger<MediaController> _logger;

        public MediaController(BlobService blobService, AppDbContext db, IFileValidator fileValidator, ILogger<MediaController> logger)
        {
            _blobService = blobService;
            _db = db;
            _fileValidator = fileValidator;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            var container = _blobService.GetContainer();
            return Ok(container.AccountName + ": NEW RESPONSE MSG");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No file uploaded.");

            var results = new List<string>();

            foreach (var file in files)
            {
                var url = await _blobService.UploadAsync(file);
                results.Add(url);
            }

            return Ok(results);
        }

        [HttpGet("sas")]
        public IActionResult GetSasUrl(string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
                return BadRequest("Blob name is required.");
            var sasUrl = _blobService.GenerateReadSasUrl(blobName, 10);
            return Ok(new { SasUrl = sasUrl });
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultiple([FromForm] UploadRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
                throw new FileValidationException("No files uploaded");

            var results = new UploadResult();

            foreach (var file in request.Files)
            {
                try
                {
                    _fileValidator.Validate(file);

                    var url = await _blobService.UploadAsync(
                        file,
                        request.FolderName);

                    results.Uploaded.Add(url);
                    _logger.LogInformation("Uploading file {FileName}", file.FileName);
                }
                catch (FileValidationException ex)
                {
                    results.Failed.Add(new FileError
                    {
                        FileName = file.FileName,
                        Error = ex.Message
                    });
                    _logger.LogError(ex, "upload failed for file {FileName}", file.FileName);
                }
                catch (Exception ex)
                {
                    results.Failed.Add(new FileError
                    {
                        FileName = file.FileName,
                        Error = $"Unexpected error: {ex.Message}"
                    });
                    _logger.LogError(ex, "upload failed for file {FileName}", file.FileName);
                }
            }

            //return Ok(results);
            return Ok(ApiResponse<UploadResult>
    .SuccessResponse(results, "Upload completed successfully"));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var media = await _db.MediaFiles.FindAsync(id);

            if (media == null)
                return NotFound();

            try
            {
                await _blobService.DeleteAsync(media.BlobName);

                _db.MediaFiles.Remove(media);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            { 
                return StatusCode(500, $"Error deleting media: {ex.Message}");
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMedia(int page = 1, int pageSize = 20, string folder = null, string search = null)
        { 
            var query = _db.MediaFiles.AsNoTracking().AsQueryable();            

            if(!string.IsNullOrWhiteSpace(folder))
               query = query.Where(m => m.Folder == folder);

            if(!string.IsNullOrEmpty(search))
                query = query.Where(m => m.FileName.ToLower().Contains(search.ToLower()));

            query = query.OrderByDescending(m => m.UploadedAt);

            var total = await query.CountAsync();

            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;
            pageSize = Math.Min(pageSize, 100);

            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new PagedResult<MediaFile>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = data
            };

            return Ok(result);
        }

        [HttpGet("crash")]
        public IActionResult Crash()
        {
            throw new Exception("Test Production Alert");
        }

    }
}
