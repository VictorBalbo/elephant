using System.Threading;
using System.Threading.Tasks;

namespace Take.Elephant.Specialized
{
    public class FallbackBlockingQueue<T> : ReplicationStrategy<IBlockingQueue<T>>, IBlockingQueue<T>
    {
        public FallbackBlockingQueue(IBlockingQueue<T> master, IBlockingQueue<T> slave)
            : base(master, slave, new CopyQueueSynchronizer<T>())
        {
        }

        public virtual Task EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            return ExecuteWithFallbackAsync(q => q.EnqueueAsync(item));
        }

        public virtual Task<T> DequeueOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            return ExecuteWithFallbackAsync(q => q.DequeueOrDefaultAsync());
        }

        public virtual Task<long> GetLengthAsync(CancellationToken cancellationToken = default)
        {
            return ExecuteWithFallbackAsync(q => q.GetLengthAsync());
        }
        public virtual Task<T> DequeueAsync(CancellationToken cancellationToken)
        {
            return ExecuteWithFallbackAsync(q => q.DequeueAsync(cancellationToken));
        }
    }
}