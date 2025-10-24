using Bulk_Export_POC.Models;
using Bulk_Export_POC.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bulk_Export_POC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BulkExportController : ControllerBase
    {
        private readonly QueueService<Job> _jobQueue;
        private readonly JobRegistry _jobRegistry;

        public BulkExportController(QueueService<Job> jobQueue, JobRegistry jobRegistry)
        {
            _jobQueue = jobQueue;
            _jobRegistry = jobRegistry;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
            Directory.CreateDirectory(uploadDir);

            string savedPath = Path.Combine(uploadDir, Guid.NewGuid() + Path.GetExtension(file.FileName));

            await using var stream = System.IO.File.Create(savedPath);
            await file.CopyToAsync(stream);

            string outputFolderPath = Path.Combine(Path.GetDirectoryName(savedPath) ?? ".", "Output");
            Directory.CreateDirectory(outputFolderPath);

            var job = new Job
            {
                FilePath = savedPath,
                OutputFolderPath = outputFolderPath
            };

            _jobRegistry.Add(job);

            bool enqueued = _jobQueue.Enqueue(job);

            if (!enqueued)
                return StatusCode(500, "Failed to queue the job.");

            return Ok(new { jobId = job.Id, status = job.Status.ToString() });
        }

        [HttpPost("{id}/cancel")]
        public IActionResult CancelJob(Guid id)
        {
            bool result = _jobRegistry.TryCancel(id);
            if (!result) return NotFound("Job not found or already completed/cancelled.");
            return Ok("Cancellation requested.");
        }
    }
}