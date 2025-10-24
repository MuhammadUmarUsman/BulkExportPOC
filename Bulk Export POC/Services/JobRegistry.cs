using Bulk_Export_POC.Models;
using Bulk_Export_POC.Models.Enums;
using System.Collections.Concurrent;

namespace Bulk_Export_POC.Services
{
    public class JobRegistry
    {
        private readonly ConcurrentDictionary<Guid, Job> _jobs = new();

        public Job Add(Job job)
        {
            _jobs[job.Id] = job;
            return job;
        }

        public bool TryCancel(Guid id)
        {
            if (_jobs.TryGetValue(id, out var job))
            {
                if (job.Status == JobStatus.Completed || job.Status == JobStatus.Cancelled) return false;

                job.Status = JobStatus.Cancelled;
                job.CompletedAt = DateTimeOffset.UtcNow;
                job.Cancellation.Cancel();

                return true;
            }
            return false;
        }

        public void Update(Job job)
        {
            _jobs[job.Id] = job;
        }
    }
}
