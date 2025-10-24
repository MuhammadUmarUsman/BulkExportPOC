using Bulk_Export_POC.Models;
using Bulk_Export_POC.Models.Enums;
using Bulk_Export_POC.Services;

namespace Bulk_Export_POC
{
    public class BackgroundJobProcessor : BackgroundService
    {
        private readonly QueueService<Job> _jobQueue;
        private readonly ResourceJsonExportService _jsonExportService;
        private readonly JobRegistry _jobRegistry;

        public BackgroundJobProcessor(QueueService<Job> jobQueue, ResourceJsonExportService jsonExportService, JobRegistry jobRegistry)
        {
            _jobQueue = jobQueue;
            _jsonExportService = jsonExportService;
            _jobRegistry = jobRegistry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("BackgroundJobProcessor started");
            try
            {
                while (!stoppingToken.IsCancellationRequested && await _jobQueue.WaitToReadAsync(stoppingToken))
                {
                    Job job = await _jobQueue.Dequeue(stoppingToken);
                    Console.WriteLine($"Dequeued file: {job.FilePath}");

                    if (job.Cancellation.IsCancellationRequested || job.Status == JobStatus.Cancelled)
                    {
                        Console.WriteLine($"Job Id: {job.Id} was Cancelled");
                        continue;
                    }

                    using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, job.Cancellation.Token);
                    CancellationToken ct = linkedCts.Token;

                    try
                    {
                        job.Status = JobStatus.Running;
                        job.StartedAt = DateTimeOffset.UtcNow;
                        _jobRegistry.Update(job);

                        await _jsonExportService.ProcessFileAsync(job.FilePath, job.OutputFolderPath, ct);

                        job.Status = JobStatus.Completed;
                        job.CompletedAt = DateTimeOffset.UtcNow;
                        Console.WriteLine($"Processed job: {job.Id}");
                    }
                    catch (OperationCanceledException)
                    {
                        job.Status = JobStatus.Cancelled;
                        job.CompletedAt = DateTimeOffset.UtcNow;
                        Console.WriteLine($"Cancelled job: {job.Id}");
                    }
                    catch (Exception ex)
                    {
                        job.Status = JobStatus.Failed;
                        job.Error = ex.Message;
                        job.CompletedAt = DateTimeOffset.UtcNow;
                        Console.WriteLine($"Failed job: {job.Id} - {ex}");
                    }
                    finally
                    {
                        _jobRegistry.Update(job);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
