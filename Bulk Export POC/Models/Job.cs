using Bulk_Export_POC.Models.Enums;

namespace Bulk_Export_POC.Models
{
    public class Job
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string FilePath { get; init; }
        public string OutputFolderPath { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string? Error { get; set; }

        public CancellationTokenSource Cancellation { get; } = new();
    }
}
