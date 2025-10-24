using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace Bulk_Export_POC.Services
{
    public class QueueService<T>
    {
        private readonly Channel<T> channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions());

        public bool Enqueue(T item)
        {
            return channel.Writer.TryWrite(item);
        }

        public ValueTask<T> Dequeue(CancellationToken ct = default)
        {
            return channel.Reader.ReadAsync(ct);
        }

        public ValueTask<bool> WaitToReadAsync(CancellationToken ct = default)
        {
            return channel.Reader.WaitToReadAsync(ct);
        }
    }
}
